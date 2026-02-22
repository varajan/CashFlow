Feature: Stocks

Scenario: I can buy stocks
	Given I am 'Michael Scott' user
		And I play as 'Engineer'
	When I buy 1000 shares of 'ok4u' stock with price $1 each
		And I buy 500 shares of 'on2u' stock with price $5 each
			But I get credit
	Then My Data is following:
"""
*Profession:* Engineer
*Cash:* $590
*Salary:* $4,900
*Income:* $0
*Expenses:* $3,410
*Cashflow:* $1,490

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

Scenario: I can buy profitable stocks
	Given I am 'Jordanne Walton' user
		And I play as 'Doctor'
		And I get $20,000 in cash
	When I buy 10 shares of 'CD' stock with price $1200 each
		And The cashflow is $20
	Then My Data is following:
"""
*Profession:* Doctor
*Cash:* $11,950
*Salary:* $13,200
*Income:* $200
*Expenses:* $9,650
*Cashflow:* $3,750

*Assets:*
• *CD* - 10 @ $1,200, Cashflow: $20 x 10 = $200

*Expenses:*
*Taxes:* $3,420
*Mortgage/Rent Pay:* $1,900
*School Loan:* $750
*Car Loan:* $380
*Credit Card:* $270
*Small Credit:* $50
*Other Payments:* $2,880
"""

Scenario: I can sell stocks
	Given I am 'Dwight Schrute' user
		And I play as 'Track driver'
		And I buy 1000 shares of 'OK4U' stock with price $1 each
		And I buy 500 shares of 'ON2U' stock with price $5 each
			But I get credit
		And I buy 2000 shares of 'OK4U' stock with price $5 each
			But I get credit
	When I sell 'OK4U' stock with price $100 each
	Then My Data is following:
"""
*Profession:* Track driver
*Cash:* $300,130
*Salary:* $2,500
*Income:* $0
*Expenses:* $2,820
*Cashflow:* -$320

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

Scenario: I can sell profitable stocks
	Given I am 'Camilla Nelson' user
		And I play as 'Doctor'
		And I get $20,000 in cash
	When I buy 5 shares of 'CD' stock with price $1200 each
		And The cashflow is $10
	And I buy 4 shares of '2BIG' stock with price $4000 each
		And The cashflow is $20
	But I sell 'CD' stock with price 2000 each
	Then My Data is following:
"""
*Profession:* Doctor
*Cash:* $11,950
*Salary:* $13,200
*Income:* $80
*Expenses:* $9,650
*Cashflow:* $3,630

*Assets:*
• *2BIG* - 4 @ $4,000, Cashflow: $20 x 4 = $80

*Expenses:*
*Taxes:* $3,420
*Mortgage/Rent Pay:* $1,900
*School Loan:* $750
*Car Loan:* $380
*Credit Card:* $270
*Small Credit:* $50
*Other Payments:* $2,880
"""

Scenario: I can multiply and divide stocks
	Given I am 'Jim Halpert' user
		And I play as 'Business manager'
		And I get $20,000 in cash
		And I buy 1000 shares of 'OK4U' stock with price $1 each
		And I buy 500 shares of 'ON2U' stock with price $5 each
		And I buy 2000 shares of 'OK4U' stock with price $5 each
		And I buy 100 shares of 'ON2U' stock with price $10 each
	When I multiply 'OK4U' stocks
		But I divide 'ON2U' stocks
	Then I have $7,570 in cash
		And My assets are:
"""
• *OK4U* - 2000 @ $1
• *ON2U* - 250 @ $5
• *OK4U* - 4000 @ $5
• *ON2U* - 50 @ $10
"""
