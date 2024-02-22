# tradingview-to-sheets

## TradingView Webhook to Google Sheets Gateway.

This small application creates a TradingView Webhook compatible REST endpoint on Azure Functions.
When called it appends the received message to Google Sheets, enabling users to update custom indicators near real-tme


## Setup

### Azure and Google
 - Create a free Azure account
 - Go through [Azure QuickStart for C#](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-cli-csharp?tabs=macos%2Cazure-cli) to install required dependencies
 - Enable API access to Google Sheets and obtain credentials
 - Create Google Project and enable Sheets API, then obtain credentials: [Tutorial](http://ai2.appinventor.mit.edu/reference/other/googlesheets-api-setup.html)
 - Fill in Google Sheet Document ID and Google Credentials to TVHook.cs file
 - Create new Azure Function App using `az functionapp create .... --name YOUR-AZURE-FN-NAME`
 - Deploy the function: `func azure functionapp publish YOUR-AZURE-FN-NAME`
 - Write down the URL provided on deploy

### TradingView
 - Create custom indicator or use built-in one
 - Add to sheet
 - Enable alerts for the indicator. [Tutorial](https://www.tradingview.com/support/solutions/43000529348-about-webhooks/)
 - Specify the Webhook URL in the alert's Notification: https://YOUR-AZURE-URL/api/tvhook?sheet=Sheet1
 - Make sure that the Message you send is in correct CSV format:
 ```
date,symbol,open,high,low,close,prev_close,RSI14
{{timenow}},{{ticker}},{{open}},{{high}},{{low}},{{close}},{{plot("prev_close")}},{{plot("RSI14")}}
 ```

## License

GNU AFFERO GENERAL PUBLIC LICENSE - Version 3


