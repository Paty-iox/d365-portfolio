import {
    clampScore,
    getRiskLevel,
    getFillColor,
    isCritical,
    getAriaLabel,
    RISK_LEVELS,
    COLOR_ZONES,
    TICK_MARKS
} from './riskLevels';

describe('riskLevels utility functions', () => {

    describe('clampScore', () => {
        test('returns null for null input', () => {
            expect(clampScore(null)).toBeNull();
        });

        test('returns null for undefined input', () => {
            expect(clampScore(undefined)).toBeNull();
        });

        test('clamps negative values to 0', () => {
            expect(clampScore(-10)).toBe(0);
            expect(clampScore(-1)).toBe(0);
        });

        test('clamps values above 100 to 100', () => {
            expect(clampScore(150)).toBe(100);
            expect(clampScore(101)).toBe(100);
        });

        test('rounds decimal values', () => {
            expect(clampScore(50.4)).toBe(50);
            expect(clampScore(50.5)).toBe(51);
            expect(clampScore(50.6)).toBe(51);
        });

        test('returns valid scores unchanged', () => {
            expect(clampScore(0)).toBe(0);
            expect(clampScore(50)).toBe(50);
            expect(clampScore(100)).toBe(100);
        });
    });

    describe('getRiskLevel', () => {
        test('returns null for null score', () => {
            expect(getRiskLevel(null)).toBeNull();
        });

        test('returns Low Risk for scores 0-25', () => {
            expect(getRiskLevel(0)?.label).toBe('Low Risk');
            expect(getRiskLevel(25)?.label).toBe('Low Risk');
        });

        test('returns Medium Risk for scores 26-50', () => {
            expect(getRiskLevel(26)?.label).toBe('Medium Risk');
            expect(getRiskLevel(50)?.label).toBe('Medium Risk');
        });

        test('returns High Risk for scores 51-75', () => {
            expect(getRiskLevel(51)?.label).toBe('High Risk');
            expect(getRiskLevel(75)?.label).toBe('High Risk');
        });

        test('returns Critical Risk for scores 76-100', () => {
            expect(getRiskLevel(76)?.label).toBe('Critical Risk');
            expect(getRiskLevel(100)?.label).toBe('Critical Risk');
        });

        test('returns correct colors for each level', () => {
            expect(getRiskLevel(10)?.primaryColor).toBe('#22C55E');
            expect(getRiskLevel(40)?.primaryColor).toBe('#EAB308');
            expect(getRiskLevel(60)?.primaryColor).toBe('#F97316');
            expect(getRiskLevel(90)?.primaryColor).toBe('#EF4444');
        });
    });

    describe('getFillColor', () => {
        test('returns green for scores 0-25', () => {
            expect(getFillColor(0)).toBe(COLOR_ZONES.green);
            expect(getFillColor(25)).toBe(COLOR_ZONES.green);
        });

        test('returns yellow for scores 26-50', () => {
            expect(getFillColor(26)).toBe(COLOR_ZONES.yellow);
            expect(getFillColor(50)).toBe(COLOR_ZONES.yellow);
        });

        test('returns orange for scores 51-75', () => {
            expect(getFillColor(51)).toBe(COLOR_ZONES.orange);
            expect(getFillColor(75)).toBe(COLOR_ZONES.orange);
        });

        test('returns red for scores 76-100', () => {
            expect(getFillColor(76)).toBe(COLOR_ZONES.red);
            expect(getFillColor(100)).toBe(COLOR_ZONES.red);
        });
    });

    describe('isCritical', () => {
        test('returns false for null', () => {
            expect(isCritical(null)).toBe(false);
        });

        test('returns false for scores <= 75', () => {
            expect(isCritical(0)).toBe(false);
            expect(isCritical(50)).toBe(false);
            expect(isCritical(75)).toBe(false);
        });

        test('returns true for scores > 75', () => {
            expect(isCritical(76)).toBe(true);
            expect(isCritical(100)).toBe(true);
        });
    });

    describe('getAriaLabel', () => {
        test('returns no score message for null', () => {
            expect(getAriaLabel(null)).toBe('Fraud Risk Score: No score available');
        });

        test('returns score with risk level label', () => {
            expect(getAriaLabel(10)).toBe('Fraud Risk Score: 10 - Low Risk');
            expect(getAriaLabel(40)).toBe('Fraud Risk Score: 40 - Medium Risk');
            expect(getAriaLabel(60)).toBe('Fraud Risk Score: 60 - High Risk');
            expect(getAriaLabel(90)).toBe('Fraud Risk Score: 90 - Critical Risk');
        });
    });

    describe('Constants', () => {
        test('RISK_LEVELS has 4 levels', () => {
            expect(RISK_LEVELS).toHaveLength(4);
        });

        test('TICK_MARKS are at expected positions', () => {
            expect(TICK_MARKS).toEqual([0, 25, 50, 75, 100]);
        });

        test('COLOR_ZONES has all required colors', () => {
            expect(COLOR_ZONES.green).toBeDefined();
            expect(COLOR_ZONES.yellow).toBeDefined();
            expect(COLOR_ZONES.orange).toBeDefined();
            expect(COLOR_ZONES.red).toBeDefined();
        });
    });
});
