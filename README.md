# Dynamics 365 & Power Platform Portfolio

[![Tests](https://github.com/Paty-iox/D365-Portfolio/actions/workflows/tests.yml/badge.svg)](https://github.com/Paty-iox/D365-Portfolio/actions/workflows/tests.yml)
[![Build Plugins](https://github.com/Paty-iox/D365-Portfolio/actions/workflows/build-plugins.yml/badge.svg)](https://github.com/Paty-iox/D365-Portfolio/actions/workflows/build-plugins.yml)

A collection of end-to-end solutions demonstrating Dynamics 365, Power Platform, and Azure integration capabilities.

---

## Projects

### [Apex Claims](./ApexClaims/)
**Insurance Claims Management System**

[![Demo Video](https://img.shields.io/badge/Demo-YouTube-red?logo=youtube)](https://youtu.be/v14AGGMQdQw)

An insurance claims processing solution with fraud detection, geolocation services, and weather data integration.

| Component | Technology |
|-----------|------------|
| Custom Entities | Claims, Policies, Incidents |
| Plugins | C# (.NET 4.6.2) - Geocoding, Weather lookup |
| PCF Control | React/TypeScript - Fraud Risk visualization |
| Azure Functions | Node.js - Fraud scoring, Geocoding, Weather API |
| Web Resources | JavaScript forms, Azure Maps integration |
| Portal | Power Pages - Customer self-service |

**Key Features:**
- Real-time fraud risk scoring with configurable thresholds
- Automatic incident location geocoding via Azure Maps
- Weather conditions capture at time of incident
- Interactive risk visualization component
- Customer portal for claim submission and tracking

---

### [Feedback Demo](./FeedbackDemo/)
**AI-Powered Customer Feedback Analytics**

[![Demo Video](https://img.shields.io/badge/Demo-YouTube-red?logo=youtube)](https://youtu.be/3y7FADlBmLs)

A feedback processing platform using Azure AI services for sentiment analysis, language translation, and automated response generation.

| Component | Technology |
|-----------|------------|
| Custom Entities | Customer Feedback, Activity Log |
| Azure Functions | Node.js - AI processing pipeline |
| Logic Apps | Dataverse updates, Email/Teams notifications |
| Azure AI | Cognitive Services, Translator, OpenAI |
| Copilot | Feedback submission and status lookup bot |
| Cloud Flows | Service Bus integration, Teams alerts |

**Key Features:**
- Multilingual support with automatic translation
- Sentiment analysis with priority escalation
- GPT-powered auto-response generation
- Daily analytics reports via email and Teams
- Copilot bot for customer self-service

---

### [Power BI Reports](./PBI/)
**Business Intelligence Dashboards**

[![Demo Video](https://img.shields.io/badge/Demo-YouTube-red?logo=youtube)](https://youtu.be/X2voYGrieos)

Power BI reports for business analytics and performance monitoring.

| Report | Description |
|--------|-------------|
| Commerce Growth & Performance | Sales trends, regional analysis, KPI tracking |

---

## Repository Structure

```
D365-Portfolio/
│
├── ApexClaims/             →  Insurance Claims Management System
│   │                          Fraud detection, geocoding, weather integration
│   │
│   │                          Tech: C# Plugins, React/TypeScript PCF,
│   │                                Node.js Functions, Power Pages, Azure Maps
│   ├── Code/
│   │   ├── AzureFunctions/       Node.js - Fraud scoring, Geocoding, Weather API
│   │   ├── PCF/                  React/TypeScript - Fraud Risk Gauge control
│   │   ├── Plugins/              C# .NET 4.6.2 - ClaimGeocoder, ClaimWeather
│   │   └── WebResources/         JavaScript - Form scripts, map integration
│   ├── Documentation/
│   ├── Portal/                   Power Pages - Customer self-service portal
│   └── Solutions/                Dataverse solution packages
│
├── FeedbackDemo/           →  AI-Powered Customer Feedback Platform
│   │                          Sentiment analysis, translation, auto-responses
│   │
│   │                          Tech: Azure OpenAI, Cognitive Services,
│   │                                Logic Apps, Power Automate, Copilot Studio
│   ├── Code/
│   │   ├── Functions/            Node.js - AI processing pipeline
│   │   ├── Infrastructure/       ARM/Bicep - Azure resource templates
│   │   └── LogicApps/            JSON - Workflow definitions
│   ├── Documentation/
│   └── Solutions/
│
├── PBI/                    →  Power BI Dashboards
│   │                          Sales analytics and KPI reporting
│   │
│   │                          Tech: Power BI, DAX, Power Query
│   └── Demo Commerce Growth and Performance/
│
├── docs/adr/               →  Architecture Decision Records
│
└── .github/workflows/      →  CI/CD (Tests, Plugin Builds)
```

## Technologies Used

**Microsoft Power Platform**
- Dynamics 365 Customer Engagement
- Power Apps (Model-driven)
- Power Automate (Cloud Flows)
- Power Pages
- Copilot Studio
- Power BI

**Azure Services**
- Azure Functions (Node.js, Consumption Plan)
- Azure Logic Apps
- Azure Cognitive Services (Text Analytics, Translator)
- Azure OpenAI Service
- Azure Service Bus
- Azure Maps

**Development**
- C# / .NET Framework 4.6.2 (Plugins)
- TypeScript / React (PCF Controls)
- JavaScript (Web Resources, Azure Functions)
- Power Platform CLI (pac)

## Author

**Patrick Y** - Dynamics 365 Consultant

- Solutions Architecture
- Power Platform Development
- Azure Integration
