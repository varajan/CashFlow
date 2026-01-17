Feature: Land

Scenario: I can buy land
	Given I am 'Kyla Parker' user
		And I play as 'Secretary'
	When I buy 10 Acrs of land with price $5,000
		But I get credit
	And I buy 20 Acrs of land with price $20,000
		But I get credit
	Then My Data is following:
"""
*Profession:* Secretary
*Cash:* $590
*Salary:* $2,500
*Income:* $0
*Expenses:* $4,020
*Cash Flow*: -$1,520

*Assets:*
• *10 Acrs* - Price: $5,000
• *20 Acrs* - Price: $20,000

*Expenses:*
*Taxes:* $460
*Mortgage/Rent Pay:* $400
*Car Loan:* $80
*Credit Card:* $60
*Small Credit:* $50
*Bank Loan:* $2,400
*Other Payments:* $570
"""

Scenario: I can sell land
	Given I am 'Gina Salazar' user
		And I play as 'Police officer'
	And I buy 10 Acrs of land with price $5,000
		But I get credit
	And I buy 20 Acrs of land with price $20,000
		But I get credit
	When I sell 10 Acrs for $150,000
	Then My Data is following:
"""
*Profession:* Police officer
*Cash:* $150,640
*Salary:* $3,000
*Income:* $0
*Expenses:* $4,280
*Cash Flow*: -$1,280

*Assets:*
• *20 Acrs* - Price: $20,000

*Expenses:*
*Taxes:* $580
*Mortgage/Rent Pay:* $400
*Car Loan:* $100
*Credit Card:* $60
*Small Credit:* $50
*Bank Loan:* $2,400
*Other Payments:* $690
"""

#@draft
#Scenario: I can see history
#	Given I am 'Gina Salazar' user
#		And I play as 'Police officer'
#	And I buy 10 Acrs of land with price $5,000
#		But I get credit
#	And I buy 20 Acrs of land with price $20,000
#		But I get credit
#	When I sell 10 Acrs for $150,000

#@draft
# rollback buy

#@draft
# rollback sell
