Feature: Coins

Scenario: I can buy coins
	Given I am 'Asiya Sutherland' user
		And I play as 'Secretary'
	When I buy 10 Pesos with price $300 each
		But I get credit
	And I buy 1 Krugerand with price $500 each
	Then My Data is following:
"""
*Profession:* Secretary
*Cash:* $90
*Salary:* $2,500
*Income:* $0
*Expenses:* $1,820
*Cash Flow*: $680

*Assets:*
• *Peso* - 10 @ $300
• *Krugerand* - 1 @ $500

*Expenses:*
*Taxes:* $460
*Mortgage/Rent Pay:* $400
*Car Loan:* $80
*Credit Card:* $60
*Small Credit:* $50
*Bank Loan:* $200
*Other Payments:* $570
"""

Scenario: I can sell coins
	Given I am 'Shaun Montes' user
		And I play as 'Secretary'
		And I buy 10 Pesos with price $300 each
		But I get credit
		And I buy 1 Krugerand with price $500 each
	When I sell Pesos for $3,000 each
		And I sell Krugerands for $5,000 each
	Then My Data is following:
"""
*Profession:* Secretary
*Cash:* $35,090
*Salary:* $2,500
*Income:* $0
*Expenses:* $1,820
*Cash Flow*: $680

*Expenses:*
*Taxes:* $460
*Mortgage/Rent Pay:* $400
*Car Loan:* $80
*Credit Card:* $60
*Small Credit:* $50
*Bank Loan:* $200
*Other Payments:* $570
"""
	And My history data is following:
"""
• Get credit: $2,000
• Buy Coins. *Peso* - 10 @ $300
• Buy Coins. *Krugerand* - 1 @ $500
• Sell Coins. *Peso* - 10 @ $3,000
• Sell Coins. *Krugerand* - 1 @ $5,000
"""

Scenario: I can rollback last buy action
	Given I am 'Aadam Stephenson' user
		And I play as 'Secretary'
	And I buy 10 Pesos with price $300 each
		But I get credit
	And I buy 1 Krugerand with price $500 each
	When I rollback last action
	Then I have $590 in cash
		And My assets are:
"""
• *Peso* - 10 @ $300
"""

Scenario: I can rollback last sell action
	Given I am 'Kieron Melton' user
		And I play as 'Secretary'
	And I buy 10 Pesos with price $300 each
	And I get credit
		But I sell Pesos for $3,000 each
	When I rollback last action
	Then I have $590 in cash
		And My assets are:
"""
• *Peso* - 10 @ $300
"""
