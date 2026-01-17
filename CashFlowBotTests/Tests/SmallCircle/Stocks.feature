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
		And I get $20,000 in cash
		And I buy 1000 shares of 'OK4U' stock with price $1 each
		And I buy 500 shares of 'ON2U' stock with price $5 each
		And I buy 2000 shares of 'OK4U' stock with price $5 each
		And I buy 100 shares of 'ON2U' stock with price $10 each
	When I multiply 'OK4U' stocks
		But I divide 'ON2U' stocks
	Then I have $7,570 in cash
		And My assets are:
		| Title | Quantity | Price |
		| OK4U  |     2000 | $1    |
		| ON2U  |      250 | $5    |
		| OK4U  |     4000 | $5    |
		| ON2U  |       50 | $10   |

Scenario: I can see transactions in history
	Given I am 'Diane Kelly' user
		And I play as 'Lawyer'
		And I buy 1000 shares of 'Ok4U' stock with price $1 each
		And I buy 500 shares of 'on2U' stock with price $5 each
			But I get credit
		And I buy 2000 shares of 'OK4U' stock with price $5 each
			But I get credit
	When I multiply 'OK4U' stocks
		But I divide 'ON2U' stocks
		And I sell 'OK4U' stock with price $50 each
	Then My history data is following:
"""
• Buy Stocks. *OK4U* - 1000 @ $1
• Get credit: $2,000
• Buy Stocks. *ON2U* - 500 @ $5
• Get credit: $10,000
• Buy Stocks. *OK4U* - 2000 @ $5
• Stocks x2. *OK4U* - 2000 @ $1
• Stocks x2. *OK4U* - 4000 @ $5
• Stocks ÷2. *ON2U* - 250 @ $5
• Sell Stocks. *OK4U* - 2000 @ $50
• Sell Stocks. *OK4U* - 4000 @ $50
"""

Scenario: I can rollback last buy transaction
	Given I am 'Hidetoshi Hasagawa' user
		And I play as 'Janitor'
		And I get $10,000 in cash
		And I buy 1000 shares of 'OK4U' stock with price $1 each
		And I buy 500 shares of 'ON2U' stock with price $5 each
	When I rollback last action
	Then I have $10,210 in cash
		And My assets are:
		| Title | Quantity | Price |
		| OK4U  |     1000 | $1    |

Scenario: I can rollback last sell transaction
	Given I am 'Jan Levinson' user
		And I play as 'Teacher'
		And I get $5,000 in cash
		And I buy 1000 shares of 'OK4U' stock with price $1 each
		And I buy 500 shares of 'ON2U' stock with price $5 each
		But I sell 'OK4U' stock with price $50 each
	When I rollback last action
	Then I have $3,010 in cash
		And My assets are:
		| Title | Quantity | Price |
		| OK4U  |     1000 | $1    |
		| ON2U  |     500  | $5    |

Scenario: I can rollback last multiply transaction
	Given I am 'Ford Taurus' user
		And I play as 'Car mechanic'
		And I get $5,000 in cash
		And I buy 1000 shares of 'OK4U' stock with price $1 each
		And I buy 500 shares of 'ON2U' stock with price $5 each
		And I buy 100 shares of 'OK4U' stock with price $5 each
	But I multiply 'OK4U' stocks
	When I rollback last action
	Then I have $2,390 in cash
		And My assets are:
		| Title | Quantity | Price |
		| OK4U  |     2000 | $1    |
		| ON2U  |     500  | $5    |
		| OK4U  |     100  | $5    |

Scenario: I can rollback last divide transaction
	Given I am 'Ryan Howard' user
		And I play as 'Pilot'
		And I get $5,000 in cash
		And I buy 1000 shares of 'OK4U' stock with price $1 each
		And I buy 500 shares of 'ON2U' stock with price $5 each
		And I buy 100 shares of 'OK4U' stock with price $5 each
	But I divide 'OK4U' stocks
	When I rollback last action
	Then I have $4,000 in cash
		And My assets are:
		| Title | Quantity | Price |
		| OK4U  |     500  | $1    |
		| ON2U  |     500  | $5    |
		| OK4U  |     100  | $5    |
