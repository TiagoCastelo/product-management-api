Feature: Stock movements
  As a warehouse operator
  I want to receive and sell stock
  So that the recorded stock always reflects what is on the shelf

  Scenario: Receiving stock increases how many units are available
    Given a product "Desk Lamp" with 10 units in stock
    When I receive 5 units of "Desk Lamp"
    Then "Desk Lamp" has 15 units in stock

  Scenario: Selling stock decreases how many units are available
    Given a product "Desk Lamp" with 10 units in stock
    When I sell 4 units of "Desk Lamp"
    Then "Desk Lamp" has 6 units in stock

  Scenario: Selling more than is available is rejected
    Given a product "Desk Lamp" with 3 units in stock
    When I sell 5 units of "Desk Lamp"
    Then the sale is rejected because there is not enough stock
    And "Desk Lamp" still has 3 units in stock
