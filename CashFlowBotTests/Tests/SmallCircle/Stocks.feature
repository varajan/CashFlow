Feature: Stocks

Scenario: I can buy stocks
	Given I am 'Michael Scott' user
		And I play as 'Engineer'
	When I buy 1000 shares of 'OK4U' stock with price '$1' each
		And I buy 500 shares of 'ON2U' stock with price '$5' each
	Then My Data is following:
"""
*Profession:* Engineer
*Cash:* $590
*Salary:* $4,900
*Income:* $0
*Expenses:* $3,410
*Cash Flow*: $1,490

*Assets:*
• *OK4U* - 1000 @ $1
• *ON2U* - 500 @ $5

*Expenses:*
*Taxes:* $1,050
*Mortgage/Rent Pay:* $700
*School Loan:* $60
*Car Loan:* $140
*Credit Card:* $120
*Small Credit:* $50
*Bank Loan:* $200
*Other Payments:* $1,090
"""

Scenario: I can sell stocks
	Given I am 'Dwight Schrute' user
		And I play as 'Track driver'
		And I buy 1000 shares of 'OK4U' stock with price '$1' each
		And I buy 500 shares of 'ON2U' stock with price '$5' each
		And I buy 2000 shares of 'OK4U' stock with price '$5' each
	When I sell 'OK4U' stock with price '$100' each
	Then My Data is following:
"""
*Profession:* Track driver
*Cash:* $300,130
*Salary:* $2,500
*Income:* $0
*Expenses:* $2,820
*Cash Flow*: -$320

*Assets:*
• *ON2U* - 500 @ $5

*Expenses:*
*Taxes:* $460
*Mortgage/Rent Pay:* $400
*Car Loan:* $80
*Credit Card:* $60
*Small Credit:* $50
*Bank Loan:* $1,200
*Other Payments:* $570
"""

Scenario: I can multiply and divide stocks
	Given I am 'Jim Halpert' user
		And I play as 'Business manager'
		And I buy 1000 shares of 'OK4U' stock with price '$1' each
		And I buy 500 shares of 'ON2U' stock with price '$5' each
		And I buy 2000 shares of 'OK4U' stock with price '$5' each
	When I multiply 'OK4U' stocks
		But I divide 'ON2U' stocks
	Then My Data is following:

"""
*Profession:* Business manager
*Cash:* $570
*Salary:* $4,600
*Income:* $0
*Expenses:* $4,130
*Cash Flow*: $470

*Assets:*
• *OK4U* - 2000 @ $1
• *ON2U* - 250 @ $5
• *OK4U* - 4000 @ $5

*Expenses:*
*Taxes:* $910
*Mortgage/Rent Pay:* $700
*School Loan:* $60
*Car Loan:* $120
*Credit Card:* $90
*Small Credit:* $50
*Bank Loan:* $1,200
*Other Payments:* $1,000
"""
