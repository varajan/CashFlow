Feature: Land

Scenario: I can buy land
	Given I am 'Kyla Parker' user
		And I play as 'Secretary'
	When I buy 10 Acrs of land with price $5,000 as Small opprotunity
		But I get credit
	And I buy 20 Acrs of land with price $20,000 as Big opprotunity
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
	And I buy 10 Acrs of land with price $5,000 as Small opprotunity
		But I get credit
	And I buy 20 Acrs of land with price $20,000 as Big opprotunity
		But I get credit
	When I sell 10 Acrs of land for $150,000
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

Scenario: I can see history
	Given I am 'Emilie Velez' user
		And I play as 'Nurse'
		And I get $30,000 in cash
	When I buy 10 Acrs of land with price $5,000 as Small opprotunity
		And I buy 20 Acrs of land with price $20,000 as Big opprotunity
	But I sell 10 Acrs of land for $150,000
	Then My history data is following:
"""
• Get $30,000
• Buy Land. *10 Acrs* - Price: $5,000
• Buy Land. *20 Acrs* - Price: $20,000
• Sell Land. *10 Acrs* - Price: $150,000
"""

Scenario: I can rollback last buy transaction
	Given I am 'Kaine Becker' user
		And I play as 'Doctor'
		And I get $25,000 in cash
	When I buy 10 Acrs of land with price $5,000 as Small opprotunity
		And I buy 20 Acrs of land with price $20,000 as Big opprotunity
	But I rollback last action
	Then I have $23,950 in cash
		And My assets are:
"""
• *10 Acrs* - Price: $5,000
"""

Scenario: I can rollback last sell transaction
	Given I am 'Dan Mccullough' user
		And I play as 'Teacher'
		And I get $25,000 in cash
	When I buy 10 Acrs of land with price $5,000 as Small opprotunity
		And I buy 20 Acrs of land with price $20,000 as Big opprotunity
		And I sell 10 Acrs of land for $150,000
	But I rollback last action
	Then I have $1,510 in cash
		And My assets are:
"""
• *10 Acrs* - Price: $5,000
• *20 Acrs* - Price: $20,000
"""
