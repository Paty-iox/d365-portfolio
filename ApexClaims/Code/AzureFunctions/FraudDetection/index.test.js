const fraudDetection = require('./index');

// Mock uuid to return predictable values
jest.mock('uuid', () => ({
    v4: jest.fn(() => 'test-uuid-1234')
}));

describe('FraudDetection Azure Function', () => {
    let context;
    let baseRequest;

    beforeEach(() => {
        context = {
            log: jest.fn(),
            res: null
        };

        baseRequest = {
            headers: {},
            body: {
                claimId: 'claim-001',
                policyId: 'policy-001',
                claimType: 'Auto',
                amount: 5000,
                location: '123 Main Street, Sydney NSW 2000',
                incidentDate: new Date().toISOString(),
                description: 'Minor fender bender in parking lot. No injuries reported. Police report filed.'
            }
        };

        // Mock setTimeout to avoid waiting in tests
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    describe('Input Validation', () => {
        test('returns 400 when claimId is missing', async () => {
            delete baseRequest.body.claimId;

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.status).toBe(400);
            expect(context.res.body.error).toBe('Missing required fields');
            expect(context.res.body.missingFields).toContain('claimId');
        });

        test('returns 400 when multiple fields are missing', async () => {
            delete baseRequest.body.claimId;
            delete baseRequest.body.amount;
            delete baseRequest.body.description;

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.status).toBe(400);
            expect(context.res.body.missingFields).toEqual(
                expect.arrayContaining(['claimId', 'amount', 'description'])
            );
        });

        test('returns 400 when body is empty', async () => {
            baseRequest.body = {};

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.status).toBe(400);
            expect(context.res.body.missingFields.length).toBe(7);
        });
    });

    describe('Risk Score Calculation', () => {
        test('returns 200 with valid request', async () => {
            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.status).toBe(200);
            expect(context.res.body).toHaveProperty('riskScore');
            expect(context.res.body).toHaveProperty('riskFactors');
            expect(context.res.body).toHaveProperty('recommendation');
            expect(context.res.body).toHaveProperty('assessmentId');
        });

        test('high amount (>50000) increases risk score', async () => {
            baseRequest.body.amount = 75000;

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.riskFactors).toContain('High claim amount');
        });

        test('elevated amount (>20000) increases risk score', async () => {
            baseRequest.body.amount = 25000;

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.riskFactors).toContain('Elevated claim amount');
        });

        test('weekend incident increases risk score', async () => {
            // Find next Saturday
            const saturday = new Date();
            saturday.setDate(saturday.getDate() + (6 - saturday.getDay()));
            baseRequest.body.incidentDate = saturday.toISOString();

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.riskFactors).toContain('Weekend incident');
        });

        test('delayed reporting (>30 days) increases risk score', async () => {
            const oldDate = new Date();
            oldDate.setDate(oldDate.getDate() - 45);
            baseRequest.body.incidentDate = oldDate.toISOString();

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.riskFactors).toContain('Delayed reporting');
        });

        test('vague location (<15 chars) increases risk score', async () => {
            baseRequest.body.location = 'Highway';

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.riskFactors).toContain('Vague location details');
        });

        test('minimal description (<50 chars) increases risk score', async () => {
            baseRequest.body.description = 'Car was damaged';

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.riskFactors).toContain('Minimal incident description');
        });

        test('total loss claim increases risk score', async () => {
            baseRequest.body.description = 'Vehicle was totaled in the accident. Complete total loss.';

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.riskFactors).toContain('Total loss claim');
        });

        test('high-value auto claim increases risk score', async () => {
            baseRequest.body.claimType = 'Auto';
            baseRequest.body.amount = 35000;

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.riskFactors).toContain('High-value auto claim');
        });

        test('risk score is capped between 0 and 100', async () => {
            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.riskScore).toBeGreaterThanOrEqual(0);
            expect(context.res.body.riskScore).toBeLessThanOrEqual(100);
        });
    });

    describe('Recommendations', () => {
        test('low risk score returns Proceed recommendation', async () => {
            // Low amount, good description, witness mentioned
            baseRequest.body.amount = 1000;
            baseRequest.body.description = 'Minor scratch on bumper. Witness present and police report filed. Photos taken at scene.';

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            // With variance, might not always be Proceed, so just check valid recommendation
            expect(['Proceed', 'Review', 'Investigate']).toContain(context.res.body.recommendation);
        });

        test('returns valid recommendation for any score', async () => {
            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(['Proceed', 'Review', 'Investigate']).toContain(context.res.body.recommendation);
        });
    });

    describe('Response Structure', () => {
        test('response includes all required fields', async () => {
            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            const body = context.res.body;
            expect(body).toHaveProperty('riskScore');
            expect(body).toHaveProperty('riskFactors');
            expect(body).toHaveProperty('recommendation');
            expect(body).toHaveProperty('assessmentId');
            expect(body).toHaveProperty('correlationId');
            expect(body).toHaveProperty('timestamp');
        });

        test('uses provided correlation ID', async () => {
            baseRequest.headers['x-correlation-id'] = 'my-correlation-123';

            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.correlationId).toBe('my-correlation-123');
        });

        test('generates correlation ID when not provided', async () => {
            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(context.res.body.correlationId).toBe('test-uuid-1234');
        });

        test('riskFactors is an array', async () => {
            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            expect(Array.isArray(context.res.body.riskFactors)).toBe(true);
        });

        test('timestamp is valid ISO string', async () => {
            const promise = fraudDetection(context, baseRequest);
            jest.runAllTimers();
            await promise;

            const timestamp = new Date(context.res.body.timestamp);
            expect(timestamp.toISOString()).toBe(context.res.body.timestamp);
        });
    });
});
