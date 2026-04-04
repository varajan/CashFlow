Feature: Bankruptcy Survive

Scenario: Sell assets to survive bankruptcy
	Given I am 'Sahar Connor' user
		And I play as 'Car mechanic'
	When I get $5,000 as a credit
		And I buy real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Big         | 4-plex | 100,000 |         6,000 |             -100 |
		And I lost my job 2 times
		And I pay $500 to Bank
	But I get a paycheck
	And I sell 4-plex asset
	Then The game is continued for me
	And My Data is following:
"""
*Profession:* Car mechanic
*Cash:* $130
*Salary:* $2,000
*Income:* $0
*Expenses:* $1,880
*Cashflow:* $120

*Expenses:*
*Taxes:* $360
*Mortgage/Rent Pay:* $300
*Car Loan:* $60
*Credit Card:* $60
*Small Credit:* $50
*Bank Loan:* $600
*Other Payments:* $450
"""
	And My history data is following:
"""
• Get credit: $5,000
• Buy Real Estate. *4-plex* - Price: $100,000, Mortgage: $94,000, Cashflow: -$100
• Get credit: $2,000
• Downsize and paying $1,780
• Get credit: $2,000
• Downsize and paying $1,980
• Pay $500
• Bankruptcy
• Sale for debts. *4-plex* - Price: $3,000
• Reduce Liabilities. Bank Loan: $3,000
"""

Scenario: Sell assets and half debts to survive bankruptcy
	Given I am 'Kaan Cole' user
		And I play as 'Car mechanic'
	When I get $5,000 as a credit
		And I buy real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Big         | 4-plex | 100,000 |         6,000 |             -100 |
		And I lost my job 3 times
		And I pay $400 to Bank
	But I get a paycheck
	And I sell 4-plex asset
	Then The game is continued for me
	And My Data is following:
"""
*Profession:* Car mechanic
*Cash:* $50
*Salary:* $2,000
*Income:* $0
*Expenses:* $1,995
*Cashflow:* $5

*Expenses:*
*Taxes:* $360
*Mortgage/Rent Pay:* $300
*Car Loan:* $30
*Credit Card:* $30
*Small Credit:* $25
*Bank Loan:* $800
*Other Payments:* $450
"""
	And My history data is following:
"""
• Get credit: $5,000
• Buy Real Estate. *4-plex* - Price: $100,000, Mortgage: $94,000, Cashflow: -$100
• Get credit: $2,000
• Downsize and paying $1,780
• Get credit: $2,000
• Downsize and paying $1,980
• Get credit: $2,000
• Downsize and paying $2,180
• Pay $400
• Bankruptcy
• Sale for debts. *4-plex* - Price: $3,000
• Reduce Liabilities. Bank Loan: $3,000
• Debt restructuring
"""

Scenario: Rollback bankruptcy actions
	Given I am 'Claire Weaver' user
		And I play as 'Car mechanic'
	When I get $5,000 as a credit
		And I buy real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Big         | 4-plex | 100,000 |         6,000 |             -100 |
		And I lost my job 3 times
		And I pay $400 to Bank
	But I get a paycheck
	And I sell 4-plex asset
	And My history data is following:
"""
• Get credit: $5,000
• Buy Real Estate. *4-plex* - Price: $100,000, Mortgage: $94,000, Cashflow: -$100
• Get credit: $2,000
• Downsize and paying $1,780
• Get credit: $2,000
• Downsize and paying $1,980
• Get credit: $2,000
• Downsize and paying $2,180
• Pay $400
• Bankruptcy
• Sale for debts. *4-plex* - Price: $3,000
• Reduce Liabilities. Bank Loan: $3,000
• Debt restructuring
"""
	But I rollback last 4 actions
	Then My Data is following:
"""
*Profession:* Car mechanic
*Cash:* $50
*Salary:* $2,000
*Income:* -$100
*Expenses:* $2,380
*Cashflow:* -$480

*Assets:*
• *4-plex* - Price: $100,000, Mortgage: $94,000, Cashflow: -$100

*Expenses:*
*Taxes:* $360
*Mortgage/Rent Pay:* $300
*Car Loan:* $60
*Credit Card:* $60
*Small Credit:* $50
*Bank Loan:* $1,100
*Other Payments:* $450
"""
