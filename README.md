# ZadElealm
I'll help you create a professional README.md file with a beautiful structure in English.

```markdown
# Islamic Educational Platform API & Admin Dashboard 🕌
A comprehensive educational platform for Islamic studies, featuring course management, tracking system, and certification services.

## 🌟 Features

### API Features
- **Authentication & Security**
  - JWT Authentication
  - Global Error Handling
  - Rate Limiting
  - CSRF Protection
  - Complete Auth System (Login, Register, Forget Password, Reset Password, Email Confirmation)

- **Course Management**
  - Organized Islamic Studies Categories
  - Detailed Course Structure
  - Video Progress Tracking
  - Course Completion System
  - Assessment Management
  - Certificate Generation

- **Notification System**
  - Course Enrollment Notifications
  - Exam Completion Alerts
  - Certificate Generation Notifications

- **External Services Integration**
  - SMTP Email Service
  - Cloudinary for Media Storage

### Admin Dashboard Features
- **User Management**
  - View/Edit/Delete Users
  - Role Management
  - User Activity Monitoring

- **Content Management**
  - Categories Management
  - Course Management (CRUD Operations)
  - Content Moderation

- **Report Management**
  - Handle User Reports
  - Email Response System
  - Issue Tracking

## 🏗️ Architecture & Design Patterns

### API Architecture
- **Onion Architecture** with four layers:
  - API Layer
  - Core Layer
  - Service Layer
  - Repository Layer

### Design Patterns
- Repository Pattern
- Generic Repository
- Specification Pattern
- Unit of Work Pattern
- CQRS Pattern

## 🛠️ Technical Stack

### Backend
- .NET 8
- Entity Framework Core
- SQL Server
- Swagger Documentation
- In-Memory Caching

### Security
- JWT Authentication
- Cookie-based Authentication (Dashboard)
- Rate Limiting
- CSRF Protection

### External Services
- Cloudinary
- SMTP Email Service

## 📋 Prerequisites
- .NET 8 SDK
- SQL Server
- Cloudinary Account
- SMTP Server Configuration

## 🚀 Getting Started

### Installation
1. Clone the repository
```bash
git clone [repository-url]
```

2. Update database connection string in `appsettings.json`

3. Apply database migrations
```bash
dotnet ef database update
```

4. Configure environment variables
```
CLOUDINARY_URL=your_cloudinary_url
SMTP_SETTINGS=your_smtp_settings
JWT_SECRET=your_jwt_secret
```

5. Run the application
```bash
dotnet run
```

## 📝 API Documentation
Access the API documentation through Swagger UI at:
```
https://[your-domain]/swagger
```

## 👥 Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License
This project is licensed under the [MIT License](LICENSE)
