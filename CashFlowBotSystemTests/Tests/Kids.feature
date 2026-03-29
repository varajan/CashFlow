Feature: Kids

Scenario: I can get kids
	Given I am 'Hayley Odom' user
		And I play as 'Engineer'
	When I get 2 kids
	Then My Data is following:
"""
*Profession:* Engineer
*Cash:* $2,090
*Salary:* $4,900
*Income:* $0
*Expenses:* $3,710
*Cashflow:* $1,190

*Expenses:*
*Taxes:* $1,050
*Mortgage/Rent Pay:* $700
*School Loan:* $60
*Car Loan:* $140
*Credit Card:* $120
*Small Credit:* $50
*Other Payments:* $1,090
*Children:* 2 ($250 per child)
*Children Expenses:* $500
"""
	And My history data is following:
"""
• Get a child
• Get a child
"""

Scenario: I can undo last action
	Given I am 'Karol Conner' user
		And I play as 'Engineer'
	When I get 2 kids
		But I rollback last action
	Then My Data is following:
"""
*Profession:* Engineer
*Cash:* $2,090
*Salary:* $4,900
*Income:* $0
*Expenses:* $3,460
*Cashflow:* $1,440

*Expenses:*
*Taxes:* $1,050
*Mortgage/Rent Pay:* $700
*School Loan:* $60
*Car Loan:* $140
*Credit Card:* $120
*Small Credit:* $50
*Other Payments:* $1,090
*Children:* 1 ($250 per child)
*Children Expenses:* $250
"""
	And My history data is following:
"""
• Get a child
"""

