Feature: Doodads

Scenario: I can buy doodads with cash
	Given I am 'Georgiana Bonilla' user
		And I play as 'Secretary'
	When I'm buying coffee on impulse
		And I pay $20 with cash
	Then I have $1,570 in cash
	And My history data is following:
"""
• Pay $20
"""

Scenario: I can buy doodads with credit card
	Given I am 'Bronwyn Medina' user
		And I play as 'Secretary'
	When I'm buying a new TV on impulse
		And I pay $2000 with credit card
	Then I have $1,590 in cash
	And My Expenses are:
"""
*Taxes:* $460
*Mortgage/Rent Pay:* $400
*Car Loan:* $80
*Credit Card:* $120
*Small Credit:* $50
*Other Payments:* $570
"""
	And My history data is following:
"""
• Pay with Credit Card - $2,000
"""

Scenario: I can buy a boat
	Given I am 'Saskia Watkins' user
		And I play as 'Secretary'
	When I buy a boat
	Then I have $590 in cash
	And My Data is following:
"""
*Profession:* Secretary
*Cash:* $590
*Salary:* $2,500
*Income:* $0
*Expenses:* $1,960
*Cash Flow*: $540

*Assets:*
• *Boat* - Price: $18,000, monthly: -$340

*Expenses:*
*Taxes:* $460
*Mortgage/Rent Pay:* $400
*Car Loan:* $80
*Credit Card:* $60
*Small Credit:* $50
*Boat Loan:* $340
*Other Payments:* $570
"""
	And My history data is following:
"""
• Buy a boat: $18,000
"""

Scenario: I can rollback doodads operations
	Given I am 'Constance Dawson' user
		And I play as 'Secretary'
	When I'm buying coffee on impulse
		And I pay $20 with cash
	And I'm buying a new TV on impulse
		And I pay $2000 with credit card
	And I buy a boat
	When I rollback last 3 actions
	Then My Data is following:
"""
*Profession:* Secretary
*Cash:* $1,590
*Salary:* $2,500
*Income:* $0
*Expenses:* $1,620
*Cash Flow*: $880

*Expenses:*
*Taxes:* $460
*Mortgage/Rent Pay:* $400
*Car Loan:* $80
*Credit Card:* $60
*Small Credit:* $50
*Other Payments:* $570
"""
