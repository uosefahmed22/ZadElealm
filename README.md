# ZadElealm

# 📖 Islamic Educational Platform API & Admin Dashboard 🕌

![Project Banner](https://github.com/uosefahmed22/ZadElealm/blob/master/ZadElealm.Apis/wwwroot/certificates/logo.png)

A comprehensive educational platform for Islamic studies featuring robust course management, student progress tracking, and certification services. Built with modern architectural patterns and secure authentication.

## 🌟 Key Features

### 🛡️ API Core Features
| Feature Category       | Highlights                                                                 |
|------------------------|----------------------------------------------------------------------------|
| **Authentication**     | JWT, CSRF Protection, Rate Limiting, Complete Auth Flow                   |
| **Course System**      | Hierarchical Categories, Progress Tracking, Assessments, Certificates      |
| **Notifications**      | Real-time alerts for enrollments, completions, and certifications          |
| **Integrations**       | Cloudinary Media Storage, SMTP Email Services, Payment Gateway Ready       |

👨‍💻 Admin Dashboard

graph TD
    A[Admin Dashboard] --> B[User Management]
    A --> C[Content Management]
    A --> D[Reporting]
    B --> B1[Role-based Access]
    C --> C1[Course CRUD]
    D --> D1[Analytics]

## 🏗️ System Architecture

### Onion Architecture Layers
1. **API Layer**: Controllers & DTOs
2. **Core Layer**: Domain Models & Business Logic
3. **Service Layer**: Application Services
4. **Repository Layer**: Data Access

### 🔧 Technical Stack

pie
    title Technology Stack
    ".NET 8" : 35
    "Entity Framework" : 25
    "SQL Server" : 20
    "Cloudinary" : 10
    "Other Services" : 10

 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server 2019+](https://www.microsoft.com/sql-server)

### Installation
# Clone repository
git clone https://github.com/uosefahmed22/ZadElealm

## ✉️ Contact  
Email: uosefahmed0022@gmail.com
