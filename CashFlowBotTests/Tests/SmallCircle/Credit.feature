Feature: Credit

Scenario: I can get a credit
	Given I am 'Harri Parrish' user
		And I play as 'Track driver'
	When I get $2000 as a credit
	Then My Data is following:
"""
*Profession:* Track driver
*Cash:* $3,630
*Salary:* $2,500
*Income:* $0
*Expenses:* $1,820
*Cashflow:* $680

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
"""

Scenario: I can undo credit operation
	Given I am 'Morgan Beasley' user
		And I play as 'Track driver'
	When I get $2000 as a credit
	But I rollback last action
	Then My Data is following:
"""
*Profession:* Track driver
*Cash:* $1,630
*Salary:* $2,500
*Income:* $0
*Expenses:* $1,620
*Cashflow:* $880

*Expenses:*
*Taxes:* $460
*Mortgage/Rent Pay:* $400
*Car Loan:* $80
*Credit Card:* $60
*Small Credit:* $50
*Other Payments:* $570
"""
