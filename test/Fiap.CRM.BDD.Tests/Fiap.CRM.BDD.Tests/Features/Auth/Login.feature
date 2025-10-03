Feature: User login

  Validates different login behaviors based on provided credentials and user type.

  Scenario: Login with valid admin credentials
    Given the user provided the username "admin"
    And the password "123456"
    When the user requests login
    Then the result should contain a JWT token

  Scenario: Login with valid regular user credentials
    Given the user provided the username "user"
    And the password "654321"
    When the user requests login
    Then the result should contain a JWT token

  Scenario: Login with invalid password for admin
    Given the user provided the username "admin"
    And the password "wrong"
    When the user requests login
    Then the result should not contain a JWT token

  Scenario: Login with invalid password for regular user
    Given the user provided the username "user"
    And the password "wrong"
    When the user requests login
    Then the result should not contain a JWT token

  Scenario: Login with non-existent user
    Given the user provided the username "doesNotExist"
    And the password "any"
    When the user requests login
    Then the result should not contain a JWT token

  Scenario: Login with empty fields
    Given the user provided the username ""
    And the password ""
    When the user requests login
    Then the result should not contain a JWT token

  Scenario: Internal error during login
    Given the user provided the username "error"
    And the password "123"
    When the user requests login
    Then an exception should be thrown

   Scenario: Login attempt with disabled account
     Given the user provided the username "inactive"
     And the password "123456"
     When the user requests login
     Then the result should not contain a JWT token
     And an error notification with message "Your account is disabled. Please contact support." should be recorded


  Scenario: Token expiration should be in the future
    Given the user provided the username "admin"
    And the password "123456"
    When the user requests login
    Then the result should contain a JWT token
    And the token expiration should be in the future