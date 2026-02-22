Feature: Downsize

Scenario: I can lost my job
	Given I am 'George Erickson' user
		And I play as 'Teacher'
	When I lost my job
	Then My Data is following:
"""
*Profession:* Teacher
*Cash:* $320
*Salary:* $3,300
*Income:* $0
*Expenses:* $2,290
*Cashflow:* $1,010

*Expenses:*
*Taxes:* $630
*Mortgage/Rent Pay:* $500
*School Loan:* $60
*Car Loan:* $100
*Credit Card:* $90
*Small Credit:* $50
*Bank Loan:* $100
*Other Payments:* $760
"""
	And My history data is following:
"""
• Get credit: $1,000
• Downsize and paying $2,190
"""

Scenario: I can undo downsize
	Given I am 'Clarence Cain' user
		And I play as 'Teacher'
	When I lost my job
		But I rollback last action
	Then I have $2,510 in cash
		And My expenses are $2,290
