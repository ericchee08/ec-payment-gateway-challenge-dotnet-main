## Request/Response Model Updates

### PostPaymentResponse and GetPaymentResponse

- Amount type was updated from int to decimal to return a representation of the current in a minior current unit. 
- Although PostPaymentResponse and GetPaymentResponse have the same structure, I chose to keep them as separate models. This makes it clearer which model is used for each endpoint and improves readability.

### PostPaymentRequest

- CardNumber type was updated from int to long due to the limitation of 10 numbers for int types in comparison to long where 19 is the limit - perfect for this requirement.
- ExpiryMonth remained as int, although if the requirement wanted to cover the edge case of a leading 0 then I would have chosen string. 
- JsonPropertyName was used to keep consistent with the JSON Format for BankSimulatorRequest. (optional)

## Controller Endpoints + Repository Pattern

<!-- TODO: Why? -->

### Endpoints

The GetPastPaymentById endpoint in a production product should ideally be protected and payment data should not be accessible by just the transacation Id - e.g. the use of API-KEY? JWT?

The ProcessPaymentAsync when called will send the entire card number to the bank for authorisation. However when storing in list (ideally database), only the last 4 card numbers should be stored due to privacy concerns.

## Helpers + Validation

### Helpers: CurrencyHelper

I chose to use GBP, USD and CNY currencies - two with the same conversions and one with different. 

### Validators: Fluent Validation

I've used FluentValidation because it gives you clear, testable validation that stays out of models + controllers. I can have them in one place and test them like normal methods by just calling ValidRequest().

The error/validation output is also well structured and I am able to use methods from CurrencyHelper to check supported currencies and IsExpiryAfterCurrentDate to consolidate the ExpiryMonth and ExpiryYear in ExpiryDate.

### Removal of Rejected enum

I've removed the rejected enum because I've designed the PostPaymentRequest endpoint to reject any invalid input via Validation - it's treated as a client error therefore does not create any payment record and does not call the bank simulator. Thinking of it from a security perspective - we shouldn't allow anything into the system that is Invalid. 

## Logging

### Serilog

## Health Checks

### Liveness check 

I've added a simple health check endpoint which takes very little time to implement - although it's not part of the requirement it's more of a demonstration of understanding that it's useful for load balancers/orchestrators (like Openshift) to decide if an instance is live and what to do with the traffic.

### Readiness check

Another feature I've added, although not part of the requirement, is a readiness check for the bank simulator. While the Bank Simulator doesn't have a health endpoint, let's assume for this scenario that it does. This readiness check helps determine whether external dependencies like the bank simulator are available and ready to serve requests before traffic is routed to the application. These checks are useful in environments orchestrated by tools like OpenShift, ensuring that services only receive traffic when all required dependencies are operational.

