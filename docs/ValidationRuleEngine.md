# ValidationRuleEngine
The `ValidationRuleEngine` class is designed to validate SQL queries against a set of predefined rules. It provides a flexible framework for registering custom validation rules and checking queries for compliance. This engine is a crucial component in ensuring the correctness and security of SQL queries, allowing developers to enforce specific standards and best practices within their applications.

## API
### Constructors
* `public ValidationRuleEngine`: Initializes a new instance of the `ValidationRuleEngine` class.

### Methods
* `public ValidationResult ValidateQuery`: Validates a SQL query against the registered rules.
	+ Parameters: The SQL query to be validated.
	+ Return Value: A `ValidationResult` object containing the validation outcome.
	+ Exceptions: May throw exceptions if the query is invalid or if an error occurs during validation.
* `public void RegisterRule`: Registers a custom validation rule with the engine.
	+ Parameters: The rule to be registered.
	+ Return Value: None.
	+ Exceptions: May throw exceptions if the rule is invalid or if an error occurs during registration.
* `public int GetRuleCount`: Retrieves the number of registered validation rules.
	+ Parameters: None.
	+ Return Value: The number of registered rules.
	+ Exceptions: None.
* `public RuleValidationResult Validate`: Validates a rule against the engine's configuration.
	+ Parameters: The rule to be validated.
	+ Return Value: A `RuleValidationResult` object containing the validation outcome.
	+ Exceptions: May throw exceptions if the rule is invalid or if an error occurs during validation.
* `public bool IsValid`: Checks if a query or rule is valid according to the registered rules.
	+ Parameters: The query or rule to be checked.
	+ Return Value: `true` if the query or rule is valid, `false` otherwise.
	+ Exceptions: None.
* `public List<string> Errors`: Retrieves a list of error messages resulting from validation.
	+ Parameters: None.
	+ Return Value: A list of error messages.
	+ Exceptions: None.
* `public List<string> Warnings`: Retrieves a list of warning messages resulting from validation.
	+ Parameters: None.
	+ Return Value: A list of warning messages.
	+ Exceptions: None.
* `public override string ToString`: Returns a string representation of the `ValidationRuleEngine` instance.
	+ Parameters: None.
	+ Return Value: A string representation of the instance.
	+ Exceptions: None.

## Usage
The following examples demonstrate how to use the `ValidationRuleEngine` class:
```csharp
// Example 1: Validating a SQL query
ValidationRuleEngine engine = new ValidationRuleEngine();
string query = "SELECT * FROM users";
ValidationResult result = engine.ValidateQuery(query);
if (result.IsValid)
{
    Console.WriteLine("Query is valid.");
}
else
{
    Console.WriteLine("Query is invalid:");
    foreach (string error in result.Errors)
    {
        Console.WriteLine(error);
    }
}

// Example 2: Registering a custom validation rule
ValidationRuleEngine engine = new ValidationRuleEngine();
CustomValidationRule rule = new CustomValidationRule();
engine.RegisterRule(rule);
string query = "SELECT * FROM users";
ValidationResult result = engine.ValidateQuery(query);
if (result.IsValid)
{
    Console.WriteLine("Query is valid.");
}
else
{
    Console.WriteLine("Query is invalid:");
    foreach (string error in result.Errors)
    {
        Console.WriteLine(error);
    }
}
```

## Notes
* The `ValidationRuleEngine` class is not thread-safe by default. If you plan to use it in a multi-threaded environment, consider implementing synchronization mechanisms to ensure thread safety.
* The `RegisterRule` method may throw exceptions if the registered rule is invalid or if an error occurs during registration. It is essential to handle these exceptions properly to prevent application crashes.
* The `ValidateQuery` method may return a `ValidationResult` object with an empty list of errors or warnings, even if the query is invalid. This can occur if the validation rules are not properly configured or if an error occurs during validation. Always check the `IsValid` property of the `ValidationResult` object to determine the validation outcome.
* The `GetRuleCount` method returns the number of registered validation rules, which can be useful for debugging or logging purposes.
* The `ToString` method returns a string representation of the `ValidationRuleEngine` instance, which can be useful for debugging or logging purposes.
