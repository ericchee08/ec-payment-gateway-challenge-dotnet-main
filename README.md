# Request/Response Model Updates

## PostPaymentResponse and GetPaymentResponse

- Amount type was updated from int to decimal to return a representation of the current in a minior current unit. 
- Although PostPaymentResponse and GetPaymentResponse have the same structure, I chose to keep them as separate models. This makes it clearer which model is used for each endpoint and improves readability.

## PostPaymentRequest

- CardNumber type was updated from int to long due to the limitation of 10 numbers for int types in comparison to long where 19 is the limit - perfect for this requirement.

- ExpiryMonth remained as int, although if the requirement wanted to cover the edge case of a leading 0 then I would have chosen string. 

- JsonPropertyName was used to keep consistent with the JSON Format for BankSimulatorRequest. (optional)

# Controller Endpoints + Repository Pattern 

<!-- TODO: WHY? -->

## Endpoints

The GetPastPaymentById endpoint in a production product should ideally be protected and payment data should not be accessible by just the transacation Id - e.g. the use of API-KEY? JWT?

The ProcessPaymentAsync when called will send the entire card number to the bank for authorisation. However when storing in list (ideally database), only the last 4 card numbers should be stored due to privacy concerns.

# Helpers + Validation

## Helpers: CurrencyHelper

I chose to use GBP, USD and CNY currencies - two with the same conversions and one with different. 

## Validators: Fluent Validation

I've used FluentValidation because it gives you clear, testable validation that stays out of models + controllers. I can have them in one place and test them like normal methods by just calling ValidRequest().

The error/validation output is also well structured and I am able to use methods from CurrencyHelper to check supported currencies and IsExpiryAfterCurrentDate to consolidate the ExpiryMonth and ExpiryYear in ExpiryDate.

## Removal of Rejected enum

I've removed the rejected enum because I've designed the PostPaymentRequest endpoint to reject any invalid input via Validation - it's treated as a client error therefore does not create any payment record and does not call the bank simulator. Thinking of it from a security perspective - we shouldn't allow anything into the system that is Invalid. 


