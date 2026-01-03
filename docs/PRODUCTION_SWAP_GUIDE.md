# Production Swap Guide

This document outlines the demo/static data used in this portfolio and provides guidance for replacing it with production implementations.

## Overview

Several components use static demo data for portfolio demonstration purposes. Each section below identifies the demo code and provides production implementation guidance.

---

## FeedbackDemo Functions

### FetchFeedbackStats

**Location:** `FeedbackDemo/Code/Functions/func-feedback-demo2/FetchFeedbackStats/index.js`

**Demo Behavior:** Returns hardcoded statistics (47 total feedback, sentiment breakdown, etc.)

**Production Swap:**
```javascript
// Replace static stats object with Dataverse Web API query:
const response = await fetch(
    `${process.env.DATAVERSE_URL}/api/data/v9.2/new_customerfeedbacks?$apply=` +
    `groupby((new_sentimentcategory),aggregate($count as count))`,
    {
        headers: {
            'Authorization': `Bearer ${accessToken}`,
            'OData-MaxVersion': '4.0',
            'OData-Version': '4.0'
        }
    }
);
const stats = await response.json();
```

**Required Configuration:**
- `DATAVERSE_URL` - Environment URL (e.g., `https://org.crm.dynamics.com`)
- OAuth token acquisition via MSAL or managed identity

---

### AnalyzeTrends

**Location:** `FeedbackDemo/Code/Functions/func-feedback-demo2/AnalyzeTrends/index.js`

**Demo Behavior:** Uses static `previousPeriod` baseline for comparison

**Production Swap:**
```javascript
// Query previous period from Dataverse:
const previousStart = new Date(currentStats.dateRange.start);
previousStart.setDate(previousStart.getDate() - 7);
const previousEnd = new Date(currentStats.dateRange.start);

const previousPeriod = await fetchStatsForPeriod(previousStart, previousEnd);
```

---

## ApexClaims Plugins

### Retry & Resilience Pattern

Both `ClaimGeocoder` and `ClaimWeather` plugins implement consistent retry logic:

```csharp
private const int MaxRetries = 3;
private const int BaseRetryDelayMs = 500;

// Exponential backoff: 500ms, 1000ms, 2000ms
int delayMs = BaseRetryDelayMs * (int)Math.Pow(2, attempt - 1);
```

**Transient errors that trigger retry:**
- `WebExceptionStatus.Timeout`
- `WebExceptionStatus.ConnectFailure`
- `WebExceptionStatus.NameResolutionFailure`
- HTTP 5xx status codes

---

## Idempotency Patterns

### ClaimGeocoder
- Checks if location field actually changed before processing
- Clears coordinates if location is empty (consistent state)
- Updates only if coordinates differ from existing values

### ClaimWeather
- Validates coordinates exist before API call
- Compares new weather conditions with existing before updating
- Skips future dates (no forecast data available)

---

## Environment Variable Configuration

All secrets are externalized via Dataverse Environment Variables:

| Component | Variable | Purpose |
|-----------|----------|---------|
| ClaimGeocoder | `new_geocodeapiurl` | Azure Function URL |
| ClaimGeocoder | `new_geocodeapikey` | Function key |
| ClaimWeather | `new_weatherapiurl` | Azure Function URL |
| ClaimWeather | `new_weatherapikey` | Function key |
| Web Resources | `new_azuremapskey` | Azure Maps key |

**Production Setup:**
1. Create Environment Variable Definitions in Dataverse solution
2. Set Environment Variable Values per environment (dev/test/prod)
3. Never hardcode secrets in code

---

## Azure Functions Local Development

1. Copy `local.settings.json.sample` to `local.settings.json`
2. Add your actual API keys
3. `local.settings.json` is in `.gitignore` - never committed

```json
{
  "Values": {
    "AZURE_MAPS_KEY": "<your-key-here>"
  }
}
```

---

## Checklist for Production Deployment

- [ ] Replace FetchFeedbackStats demo data with Dataverse queries
- [ ] Replace AnalyzeTrends baseline with historical query
- [ ] Configure all Environment Variables in target environment
- [ ] Verify retry logic handles expected failure scenarios
- [ ] Test idempotency by re-running plugins on same records
- [ ] Remove or update demo-specific comments
- [ ] Run integration tests against production-like environment
