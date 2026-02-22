Feature: SmallBusiness

Scenario: I can start small business
	Given I am 'Whitney Keller' user
		And I play as 'Business manager'
	When I start the Auto Tools company with $3000
		But I get credit
	And I start the Computer Programs company with $5000
		But I get credit
	Then My Data is following:
"""
*Profession:* Business manager
*Cash:* $70
*Salary:* $4,600
*Income:* $0
*Expenses:* $3,530
*Cashflow:* $1,070

*Assets:*
• *Auto Tools* - Price: $3,000
• *Computer Programs* - Price: $5,000

*Expenses:*
*Taxes:* $910
*Mortgage/Rent Pay:* $700
*School Loan:* $60
*Car Loan:* $120
*Credit Card:* $90
*Small Credit:* $50
*Bank Loan:* $600
*Other Payments:* $1,000
"""

Scenario: I can increase cash flow for small business
	Given I am 'Kelsie John' user
		And I play as 'Business manager'
		And I get $20,000 in cash
		And I start the Auto Tools company with $3000
		And I start the Computer Programs company with $5000
	When I increase the cash flow of my small business by $250
		And I increase the cash flow of my small business by $400
	Then My Data is following:
"""
*Profession:* Business manager
*Cash:* $14,070
*Salary:* $4,600
*Income:* $1,300
*Expenses:* $2,930
*Cashflow:* $2,970

*Assets:*
• *Auto Tools* - Price: $3,000, monthly: $650
• *Computer Programs* - Price: $5,000, monthly: $650

*Expenses:*
*Taxes:* $910
*Mortgage/Rent Pay:* $700
*School Loan:* $60
*Car Loan:* $120
*Credit Card:* $90
*Small Credit:* $50
*Other Payments:* $1,000
"""
	And My history data is following:
"""
• Get $20,000
• Start a company. *Auto Tools* - Price: $3,000
• Start a company. *Computer Programs* - Price: $5,000
• *Auto Tools* - Increase cash flow. $250
• *Computer Programs* - Increase cash flow. $250
• *Auto Tools* - Increase cash flow. $400
• *Computer Programs* - Increase cash flow. $400
"""

Scenario: I can sell small business
	Given I am 'Nevaeh Higgins' user
		And I play as 'Business manager'
		And I get $20,000 in cash
		And I start the Auto Tools company with $3000
		And I start the Computer Programs company with $5000
	When I increase the cash flow of my small business by $250
		And I increase the cash flow of my small business by $400
		But I sell Auto Tools small business for $100,000
	Then My Data is following:
"""
*Profession:* Business manager
*Cash:* $114,070
*Salary:* $4,600
*Income:* $650
*Expenses:* $2,930
*Cashflow:* $2,320

*Assets:*
• *Computer Programs* - Price: $5,000, monthly: $650

*Expenses:*
*Taxes:* $910
*Mortgage/Rent Pay:* $700
*School Loan:* $60
*Car Loan:* $120
*Credit Card:* $90
*Small Credit:* $50
*Other Payments:* $1,000
"""
	And My history data is following:
"""
• Get $20,000
• Start a company. *Auto Tools* - Price: $3,000
• Start a company. *Computer Programs* - Price: $5,000
• *Auto Tools* - Increase cash flow. $250
• *Computer Programs* - Increase cash flow. $250
• *Auto Tools* - Increase cash flow. $400
• *Computer Programs* - Increase cash flow. $400
• Sell Business. *Auto Tools* - Price: $3,000, monthly: $650
"""

Scenario: I can rollback starting a small business
	Given I am 'Kylie Salinas' user
		And I play as 'Business manager'
		And I get $20,000 in cash
		And I start the Auto Tools company with $3000
		And I start the Computer Programs company with $5000
	When I rollback last action
	Then I have $19,070 in cash
		And My assets are:
"""
• *Auto Tools* - Price: $3,000
"""

Scenario: I can rollback selling small business
	Given I am 'Alan George' user
		And I play as 'Business manager'
		And I get $20,000 in cash
		And I start the Auto Tools company with $3000
		And I start the Computer Programs company with $5000
		And I increase the cash flow of my small business by $400
	When I sell Auto Tools small business for $100,000
		But I rollback last action
	Then I have $14,070 in cash
		And My passive income is $800
		And My assets are:
"""
• *Auto Tools* - Price: $3,000, monthly: $400
• *Computer Programs* - Price: $5,000, monthly: $400
"""

Scenario: I can rollback increasing cashflow
	Given I am 'Lea Suarez' user
		And I play as 'Business manager'
		And I get $20,000 in cash
		And I start the Auto Tools company with $3000
		And I start the Computer Programs company with $5000
		And I increase the cash flow of my small business by $250
		And I increase the cash flow of my small business by $400
	When I sell Auto Tools small business for $100,000
		But I rollback last action
	Then I have $14,070 in cash
		And My passive income is $1,300
		And My assets are:
"""
• *Auto Tools* - Price: $3,000, monthly: $650
• *Computer Programs* - Price: $5,000, monthly: $650
"""
