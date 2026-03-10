### Running the API

1. **Run the Bank Simulator**

   First, make sure you have the Bank Simulator running on `http://localhost:8080`.  
   Start it via `docker-compose up`

2. **Run the Payment Gateway API**

3. **Check the API documentation**

   Go to [http://localhost:7092/swagger](http://localhost:7092/swagger)  
   This provides interactive documentation for testing endpoints.


## Configurations - secrets/endpoints 

In this task, the bank simulator endpoints are currently set directly in `appsettings`. In a real production scenario, I wouldn't hard-code these values, same if I had API keys or sensitive configuration. 

**What I'd do in production:**

- **Use Environment Variables:** Store secrets such as connection strings, API keys, and endpoints in environment variables on the hosting platform or deployment pipeline. e.g. Azure Key Vault.
- **Automated Configuration:** For endpoints that change per environment (dev, staging, prod), use environment variables to inject the correct settings at deployment.

## Request/Response Model Updates

### PostPaymentResponse and GetPaymentResponse

- Amount type was updated from int to decimal to return a representation of the currency in a minor currency unit which requires decimal places. 
- Although PostPaymentResponse and GetPaymentResponse have the same structure, I chose to keep them as separate models. This makes it clearer which model is used for each endpoint and improves readability.

### PostPaymentRequest

- CardNumber type was updated from int to long due to the limitation of 10 numbers for int types in comparison to long where 19 is the limit - perfect for this requirement of a maximum of 19 numbers.
- ExpiryMonth remained as int, although if the requirement wanted to cover the edge case of a leading 0 then I would have chosen string type.
- JsonPropertyName was used to keep consistent with the JSON Format for BankSimulatorRequest. (optional: it's a design consideration I like)

## Controller Endpoints + Repository Pattern

- I've used the repository pattern to seperate the concerns of the controller and the repository: the controller stays minimal (routing, status codes, request/response only) and all payment workflow like calling the bank, building responses, storing and retrieving payments, that all lives in the repository. 

- This makes the repository easy to unit test with a mocked bank client (no HTTP involved), and assuming we switched from the in-memory list to a real database, only the repository implementation changes, the controller and its tests stay unchanged.

### Endpoints

- The GetPastPaymentById endpoint in a production product should ideally be protected and payment data should not be easily accessible by just the transacation Id - e.g. the use of API-KEY? JWT?
- The ProcessPaymentAsync when called will send the entire card number to the bank for authorisation. However when storing in list (ideally database), only the last 4 card numbers should be stored due to privacy concerns. 

Note: I would consider better encyption approach depending on compliance requirements from the bank.

## Helpers + Validation

### Helpers: CurrencyHelper

- I chose to use GBP, USD and CNY currencies - two with the same conversions and one with different - could do with more but requirement only requires 3 max.

### Validators: Fluent Validation

- I've used FluentValidation because it gives you clear, testable validation that stays out of models + controllers. I can have them in one place and test them like normal methods by just calling ValidRequest().

- The error/validation output is also well structured and I am able to use methods from CurrencyHelper to check supported currencies and IsExpiryAfterCurrentDate to consolidate the ExpiryMonth and ExpiryYear in ExpiryDate.

### Removal of Rejected enum

- I've removed the rejected enum because I've designed the PostPaymentRequest endpoint to reject any invalid input via Validation - it's treated as a client error therefore does not create any payment record and does not call the bank simulator. Thinking of it from a security perspective - we shouldn't allow anything into the system that is Invalid. 

## Logging

### Serilog - Dev vs Prod

- Although logging wasn't a requirement, I decided to implement it to demonstrate the understanding and importance of logs for monitoring that helps with troubleshooting and general observability. I've used Serilog for its structured logging capabilities instead of the plain text logging that comes with default logging. Each log event includes named properties (such as PaymentId, TraceId, StatusCode). Assuming we are using a log viewer like elastic this allows easier searching, filtering, and request tracing. 

- In production, I decided only to log events that are important, like errors, failed requests, or requests that are unusually slow (over X seconds - in this case i've set to 1 second). 
In development, logging is more verbose to help with debugging.

## Health Checks

### Liveness check 

- I've added a simple health check endpoint which takes very little time to implement - although it's not part of the requirement it's more of a demonstration of understanding that it's useful for load balancers/orchestrators (like Openshift) to decide if an instance is live and what to do with the traffic.

### Readiness check

- Another feature I've added, although not part of the requirement, is a readiness check for the bank simulator. While the Bank Simulator doesn't have a health endpoint, let's assume for this scenario that it does. This readiness check helps determine whether external dependencies like the bank simulator are available and ready to serve requests before traffic is routed to the application. These checks are useful in environments orchestrated by tools like OpenShift, ensuring that services only receive traffic when all required dependencies are operational.

