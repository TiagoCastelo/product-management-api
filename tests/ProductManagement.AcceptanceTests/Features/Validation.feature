Feature: Validation
  As a store manager
  I want invalid product details to be rejected
  So that the catalogue only ever holds well-formed data

  Scenario Outline: A product with invalid details is rejected
    When I try to add a product with <field> set to "<value>"
    Then the request is rejected with a validation error for "<field>"

    Examples:
      | field | value             |
      | sku   | lowercase-sku     |
      | name  |                   |
      | name  | (101 characters)  |
      | price | 0                 |
      | price | 19.999            |
      | stock | -1                |
