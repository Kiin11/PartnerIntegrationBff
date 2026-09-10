# Partner Integration BFF (.NET 8)

Một microservice **Backend-For-Frontend (BFF)** sẵn sàng cho môi trường sản xuất, được xây dựng bằng **.NET 8** để xử lý việc tiếp nhận giao dịch từ đối tác, xác minh bên ngoài với khả năng phục hồi/thử lại và điều phối hàng đợi tin nhắn bất đồng bộ.
---

## 1. Architectural Decisions & Design Principles

Dự án áp dụng các nguyên tắc **Clean Architecture** và sự phân tách các mối quan tâm để đảm bảo tính liên kết lỏng lẻo, khả năng kiểm thử và khả năng bảo trì.

```text
PartnerIntegrationBff/
├── PartnerIntegrationBff.sln
├── docker-compose.yml                     # Multi-container orchestration (API + RabbitMQ)
├── PartnerIntegrationBff/                 # Main API project
│   ├── Clients/                           # External HTTP integration (PartnerVerificationClient)
│   ├── Constants/                         # Domain constants (CurrencyConstant)
│   ├── Controllers/                       # Endpoints (PartnerTransactions, PartnerVerificationClient)
│   ├── Middleware/                        # GlobalExceptionHandler (IExceptionHandler), DI
│   ├── Interfaces/                        # Abstractions (IPartnerVerificationClient, IMessageProducer)
│   ├── Models/                            # DTOs, Requests, Enriched Messages
│   ├── Services/                          # Infrastructure implementations (RabbitMqMessageProducer)
│   ├── Validators/                        # FluentValidation rules
│   ├── Dockerfile                         # Multi-stage container build
│   └── DependencyInjection.cs             # Centralized DI registrations
└── PartnerIntegration.UnitTests/          # Unit tests (Validators, Resilience, Clients, Controllers)

```

## 2. Prerequisites
.NET 8.0 SDK

Docker

## 3. How to Run the Project
Run docker:
docker compose up --build -d

Stop:
docker compose down

## 4. Security Considerations
Để bảo đảm bảo mật chúng ta có thể sử dụng 1 số phương pháp bảo vệ Endpoint sau:
1. OAuth 2.0 / JWT : Sử dụng luồng xác thực thông tin đăng nhập máy khách OAuth 2.0 với phạm vi chi tiết (ví dụ: transactions:write).
2. Rate Limiting: Triển khai tính năng giới hạn tỷ lệ truy cập ASP.NET Core được phân vùng theo PartnerId hoặc IP để chống lại các cuộc tấn công DoS và các đợt tăng đột biến lưu lượng truy cập không lành mạnh.
3. HMAC Signature Verification: Yêu cầu các đối tác cung cấp tiêu đề X-Signature được tính toán dưới dạng HMAC-SHA256(payload, secret_key) để đảm bảo tính xác thực của yêu cầu và tính toàn vẹn của thông điệp.