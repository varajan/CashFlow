Feature: Friends

Background:
	Given Few players:
	| Name              | Profession |
	| Bronwyn Berry     | Doctor     |
	| Damien Washington | Pilot      |
	| Bryony Morrison   | Teacher    |
	| Kelsie Humphrey   | Lawyer     |

	When Bronwyn Berry get $1000 as a credit
		And Damien Washington get $1000 in cash
		And Bryony Morrison get $2,000 in cash
		And Bryony Morrison get $3,000 in cash
		And Bryony Morrison get $5,000 in cash
		And Bryony Morrison buys real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Small       | 2/1    |  60,000 |         2,000 |            1,000 |
		| Big         | 8-plex | 150,000 |         5,000 |            1,500 |
		And Bryony Morrison goes to the Big Circle

Scenario: I can see my friends
	When Kelsie Humphrey says 'Friends'
	Then Kelsie Humphrey can see friends:
"""
*On Small circle:*
• Bronwyn Berry
• Damien Washington

*On Big circle:*
• Bryony Morrison
"""

Scenario: I can see friend's info on small circle
	When Kelsie Humphrey says 'Friends'
		And Kelsie Humphrey says 'Damien Washington'
	Then Kelsie Humphrey can see details:
"""
*Profession:* Pilot
*Cash:* $4,000
*Salary:* $9,500
*Income:* $0
*Expenses:* $6,900
*Cashflow:* $2,600
"""
	And Kelsie Humphrey can see history details:
"""
• Get $1,000
"""

Scenario: I can see friend's info on big circle
	When Kelsie Humphrey says 'Friends'
		And Kelsie Humphrey says 'Bryony Morrison'
	Then Kelsie Humphrey can see details:
"""
*Profession:* Teacher
*Cash:* $254,510
Initial Cashflow: $250,000
Current Cashflow: $250,000
Target Cashflow: $300,000
"""
	And Kelsie Humphrey can see history details:
"""
• Go to Big Circle
• Buy Real Estate. *8-plex* - Price: $150,000, Mortgage: $145,000, Cashflow: $1,500
• Buy Real Estate. *2/1* - Price: $60,000, Mortgage: $58,000, Cashflow: $1,000
• Get $5,000
• Get $3,000
"""
