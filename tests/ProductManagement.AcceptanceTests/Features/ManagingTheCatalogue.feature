Feature: Managing the catalogue
  As a store manager
  I want to create, view, update and delete products
  So that the catalogue reflects what we actually sell

  Scenario: Creating a product adds it to the catalogue with a unique identifier
    When I add a new product "Standing Desk" priced at 249.99 with 12 units in stock
    Then "Standing Desk" is in the catalogue with a six-digit product id
    And "Standing Desk" is priced at 249.99 with 12 units in stock

  Scenario: Viewing a product shows its current details
    Given a product "Desk Lamp" priced at 19.99 with 30 units in stock
    When I view "Desk Lamp"
    Then "Desk Lamp" is priced at 19.99 with 30 units in stock

  Scenario: Renaming and repricing a product updates its details
    Given a product "Office Chair" priced at 89.00 with 5 units in stock
    When I rename "Office Chair" to "Ergonomic Office Chair" and change its price to 109.00
    Then "Ergonomic Office Chair" is priced at 109.00 with 5 units in stock

  Scenario: Deleting a product removes it from the catalogue
    Given a product "Whiteboard" priced at 45.00 with 8 units in stock
    When I remove "Whiteboard" from the catalogue
    Then "Whiteboard" can no longer be found

  Scenario: Creating a product with a SKU already in use is rejected
    Given a product "Notebook A5" priced at 4.50 with 100 units in stock
    When I add another product with the same SKU as "Notebook A5"
    Then the request is rejected because the SKU is already in use
