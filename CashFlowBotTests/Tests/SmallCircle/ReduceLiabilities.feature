Feature: ReduceLiabilities

Scenario: I can pay off my Mortgage
	Given I am 'Lea Cooper' user
		And I play as 'Pilot'
		And I get $200,000 in cash
	When I pay off my Mortgage
	Then My Data is following:
"""
*Profession:* Pilot
*Cash:* $60,000
*Salary:* $9,500
*Income:* $0
*Expenses:* $5,570
*Cash Flow*: $3,930

*Expenses:*
*Taxes:* $2,350
*Car Loan:* $300
*Credit Card:* $660
*Small Credit:* $50
*Other Payments:* $2,210
"""
	And My history data is following:
"""
• Get $200,000
• Reduce Liabilities. Mortgage: $143,000
"""

Scenario: I can pay off my Car Loan
	Given I am 'Marshall Paul' user
		And I play as 'Pilot'
		And I get $200,000 in cash
	When I pay off my Car Loan
	Then My Data is following:
"""
*Profession:* Pilot
*Cash:* $188,000
*Salary:* $9,500
*Income:* $0
*Expenses:* $6,600
*Cash Flow*: $2,900

*Expenses:*
*Taxes:* $2,350
*Mortgage/Rent Pay:* $1,330
*Credit Card:* $660
*Small Credit:* $50
*Other Payments:* $2,210
"""
	And My history data is following:
"""
• Get $200,000
• Reduce Liabilities. Car Loan: $15,000
"""

Scenario: I can pay off my Credit Card
	Given I am 'Uzair Riggs' user
		And I play as 'Pilot'
		And I get $200,000 in cash
	When I pay off my Credit Card
	Then My Data is following:
"""
*Profession:* Pilot
*Cash:* $181,000
*Salary:* $9,500
*Income:* $0
*Expenses:* $6,240
*Cash Flow*: $3,260

*Expenses:*
*Taxes:* $2,350
*Mortgage/Rent Pay:* $1,330
*Car Loan:* $300
*Small Credit:* $50
*Other Payments:* $2,210
"""
	And My history data is following:
"""
• Get $200,000
• Reduce Liabilities. Credit Card: $22,000
"""

Scenario: I can pay off my Small Credit
	Given I am 'Rihanna Wang' user
		And I play as 'Pilot'
		And I get $200,000 in cash
	When I pay off my Small Credit
	Then My Data is following:
"""
*Profession:* Pilot
*Cash:* $202,000
*Salary:* $9,500
*Income:* $0
*Expenses:* $6,850
*Cash Flow*: $2,650

*Expenses:*
*Taxes:* $2,350
*Mortgage/Rent Pay:* $1,330
*Car Loan:* $300
*Credit Card:* $660
*Other Payments:* $2,210
"""
	And My history data is following:
"""
• Get $200,000
• Reduce Liabilities. Small Credit: $1,000
"""

Scenario: I can pay off my Bank Loan fully
	Given I am 'Logan Lee' user
		And I play as 'Pilot'
		And I get 5000 as a credit
	When I pay off $5000 of my Bank Loan
	Then My Data is following:
"""
*Profession:* Pilot
*Cash:* $3,000
*Salary:* $9,500
*Income:* $0
*Expenses:* $6,900
*Cash Flow*: $2,600

*Expenses:*
*Taxes:* $2,350
*Mortgage/Rent Pay:* $1,330
*Car Loan:* $300
*Credit Card:* $660
*Small Credit:* $50
*Other Payments:* $2,210
"""
	And My history data is following:
"""
• Get credit: $5,000
• Reduce Liabilities. Bank Loan: $5,000
"""

Scenario: I can pay off my Bank Loan partially
	Given I am 'Leighton Hayes' user
		And I play as 'Pilot'
		And I get 5000 as a credit
	When I pay off $2000 of my Bank Loan
	Then My Data is following:
"""
*Profession:* Pilot
*Cash:* $6,000
*Salary:* $9,500
*Income:* $0
*Expenses:* $7,200
*Cash Flow*: $2,300

*Expenses:*
*Taxes:* $2,350
*Mortgage/Rent Pay:* $1,330
*Car Loan:* $300
*Credit Card:* $660
*Small Credit:* $50
*Bank Loan:* $300
*Other Payments:* $2,210
"""
	And My history data is following:
"""
• Get credit: $5,000
• Reduce Liabilities. Bank Loan: $2,000
"""

Scenario: I can pay off my Boat Loan
	Given I am 'Tommy-Lee Archer' user
		And I play as 'Pilot'
		And I get $200,000 in cash
	But I buy a boat
	When I pay off my Boat Loan
	Then My Data is following:
"""
*Profession:* Pilot
*Cash:* $185,000
*Salary:* $9,500
*Income:* $0
*Expenses:* $6,900
*Cash Flow*: $2,600

*Assets:*
• *Boat* - Price: $18,000

*Expenses:*
*Taxes:* $2,350
*Mortgage/Rent Pay:* $1,330
*Car Loan:* $300
*Credit Card:* $660
*Small Credit:* $50
*Other Payments:* $2,210
"""
	And My history data is following:
"""
• Get $200,000
• Buy a boat: $18,000
• Reduce Liabilities. Boat Loan: $17,000
"""

Scenario: I can rollback transactions
	Given I am 'Jak Sosa' user
		And I play as 'Pilot'
		And I get $200,000 in cash
	But I get 2000 as a credit
		And I buy a boat
	When I pay off my Mortgage
		And I pay off my Car Loan
		And I pay off my Credit Card
		And I pay off my Small Credit
		And I pay off $2000 of my Bank Loan
		And I pay off my Boat Loan
	But I rollback last 6 actions
	Then My Data is following:
"""
*Profession:* Pilot
*Cash:* $204,000
*Salary:* $9,500
*Income:* $0
*Expenses:* $7,440
*Cash Flow*: $2,060

*Assets:*
• *Boat* - Price: $18,000, monthly: -$340

*Expenses:*
*Taxes:* $2,350
*Mortgage/Rent Pay:* $1,330
*Car Loan:* $300
*Credit Card:* $660
*Small Credit:* $50
*Bank Loan:* $200
*Boat Loan:* $340
*Other Payments:* $2,210
"""
	And My history data is following:
"""
• Get $200,000
• Get credit: $2,000
• Buy a boat: $18,000
"""
