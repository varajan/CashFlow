Feature: Business

Scenario: I can buy businesses
	Given I am 'Inayah Hodge' user
		And I play as 'Car mechanic'
		And I get $200,000 in cash
	When I buy businesses:
		| Title               | Price   | First Payment | Monthly Cashflow |
		| Car wash            |  20,000 |        20,000 |              800 |
		| Company             |  25,000 |        20,000 |             1000 |
		| Enterprise          |  30,000 |        25,000 |             1500 |
		| Limited partnership | 100,000 |        30,000 |             1600 |
		| Passage             | 150,000 |        40,000 |            1,800 |
		| Pizzeria            | 500,000 |        50,000 |             2500 |
	Then My Data is following:
"""
*Profession:* Car mechanic
*Cash:* $16,390
*Salary:* $2,000
*Income:* $9,200
*Expenses:* $1,280
*Cash Flow*: $9,920

*Assets:*
• *Car Wash* - Price: $20,000, Cash Flow: $800
• *Company* - Price: $25,000, Mortgage: $5,000, Cash Flow: $1,000
• *Enterprise* - Price: $30,000, Mortgage: $5,000, Cash Flow: $1,500
• *Limited Partnership* - Price: $100,000, Mortgage: $70,000, Cash Flow: $1,600
• *Passage* - Price: $150,000, Mortgage: $110,000, Cash Flow: $1,800
• *Pizzeria* - Price: $500,000, Mortgage: $450,000, Cash Flow: $2,500

*Expenses:*
*Taxes:* $360
*Mortgage/Rent Pay:* $300
*Car Loan:* $60
*Credit Card:* $60
*Small Credit:* $50
*Other Payments:* $450
"""

Scenario: I can sell businesses
	Given I am 'Timothy Fry' user
		And I play as 'Car mechanic'
		And I get $200,000 in cash
	When I buy businesses:
		| Title               | Price   | First Payment | Monthly Cashflow |
		| Car wash            |  20,000 |        20,000 |              800 |
		| Company             |  25,000 |        20,000 |             1000 |
		| Enterprise          |  30,000 |        25,000 |             1500 |
		| Limited partnership | 100,000 |        30,000 |             1600 |
		| Passage             | 150,000 |        40,000 |            1,800 |
		| Pizzeria            | 500,000 |        50,000 |             2500 |
		But I sell Car wash for $100,000
		And I sell Passage for $200,000
	Then My Data is following:
"""
*Profession:* Car mechanic
*Cash:* $206,390
*Salary:* $2,000
*Income:* $6,600
*Expenses:* $1,280
*Cash Flow*: $7,320

*Assets:*
• *Company* - Price: $25,000, Mortgage: $5,000, Cash Flow: $1,000
• *Enterprise* - Price: $30,000, Mortgage: $5,000, Cash Flow: $1,500
• *Limited Partnership* - Price: $100,000, Mortgage: $70,000, Cash Flow: $1,600
• *Pizzeria* - Price: $500,000, Mortgage: $450,000, Cash Flow: $2,500

*Expenses:*
*Taxes:* $360
*Mortgage/Rent Pay:* $300
*Car Loan:* $60
*Credit Card:* $60
*Small Credit:* $50
*Other Payments:* $450
"""
	And My history data is following:
"""
• Get $200,000
• Buy Business. *Car Wash* - Price: $20,000, Cash Flow: $800
• Buy Business. *Company* - Price: $25,000, Mortgage: $5,000, Cash Flow: $1,000
• Buy Business. *Enterprise* - Price: $30,000, Mortgage: $5,000, Cash Flow: $1,500
• Buy Business. *Limited Partnership* - Price: $100,000, Mortgage: $70,000, Cash Flow: $1,600
• Buy Business. *Passage* - Price: $150,000, Mortgage: $110,000, Cash Flow: $1,800
• Buy Business. *Pizzeria* - Price: $500,000, Mortgage: $450,000, Cash Flow: $2,500
• Sell Business. *Car Wash* - Price: $100,000
• Sell Business. *Passage* - Price: $200,000
"""

Scenario: I can rollback buy action
	Given I am 'Gabriela Johns' user
		And I play as 'Car mechanic'
		And I get $50,000 in cash
	When I buy businesses:
		| Title               | Price   | First Payment | Monthly Cashflow |
		| Car wash            |  20,000 |        20,000 |              800 |
		| Company             |  25,000 |        20,000 |             1000 |
	But I rollback last action
	Then I have $31,390 in cash
		And My passive in come is $800
		And My assets are:
"""
• *Car Wash* - Price: $20,000, Cash Flow: $800
"""

Scenario: I can rollback sell action
	Given I am 'Sidney Nielsen' user
		And I play as 'Car mechanic'
		And I get $50,000 in cash
		And I buy businesses:
		| Title               | Price   | First Payment | Monthly Cashflow |
		| Car wash            |  20,000 |        20,000 |              800 |
		| Company             |  25,000 |        20,000 |             1000 |
	When I sell Car wash for $30,000
	But I rollback last action
	Then I have $11,390 in cash
		And My passive in come is $1,800
		And My assets are:
"""
• *Car Wash* - Price: $20,000, Cash Flow: $800
• *Company* - Price: $25,000, Mortgage: $5,000, Cash Flow: $1,000
"""
