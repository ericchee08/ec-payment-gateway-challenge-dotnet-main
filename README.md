Request/Response Model Updates

PostPaymentResponse and GetPaymentResponse

- Amount type was updated from int to decimal to return a representation of the current in a minior current unit. 

PostPaymentRequest

- CardNumber type was updated from int to long due to the limitation of 10 numbers for int types in comparison to long where 19 is the limit - perfect for this requirement.

- ExpiryMonth remained as int, although if the requirement wanted to cover the edge case of a leading 0 then I would have chosen string. 

- JsonPropertyName was used to keep consistent with the JSON Format for BankSimulatorRequest. (optional)

Repository Pattern

<!-- TODO: WHY? -->

Endpoints

The GetPastPaymentById endpoint in a production product should ideally be protected and payment data should not be accessible by just the transacation Id - e.g. the use of API-KEY? JWT?