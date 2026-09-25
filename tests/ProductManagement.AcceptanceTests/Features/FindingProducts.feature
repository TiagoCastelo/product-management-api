Feature: Finding products
  As a store manager
  I want to search the catalogue by name and by stock level
  So that I can quickly find the products I am looking for

  Scenario: Searching by a partial, case-insensitive name finds matching products
    Given a product "Wireless Mouse" priced at 25.00 with 10 units in stock
    And a product "Gaming MOUSE" priced at 60.00 with 4 units in stock
    And a product "USB Keyboard" priced at 30.00 with 15 units in stock
    When I search the catalogue for "mouse"
    Then the results include "Wireless Mouse"
    And the results include "Gaming MOUSE"
    And the results do not include "USB Keyboard"

  Scenario: Listing products within a stock range includes only products in that range
    Given a product "Low Stock Widget" with 4 units in stock
    And a product "In Range Widget A" with 5 units in stock
    And a product "In Range Widget B" with 10 units in stock
    And a product "Above Range Widget" with 11 units in stock
    When I list products with stock between 5 and 10
    Then the results include "In Range Widget A"
    And the results include "In Range Widget B"
    And the results do not include "Low Stock Widget"
    And the results do not include "Above Range Widget"

  Scenario: Requesting a stock range where the minimum exceeds the maximum is rejected
    When I list products with stock between 10 and 5
    Then the request is rejected because the range is invalid
