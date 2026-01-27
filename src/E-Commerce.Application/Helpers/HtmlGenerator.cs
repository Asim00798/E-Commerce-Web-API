namespace E_Commerce.Domain.Helpers
{
    public static class HtmlGenerator
    {
        public static string GenerateHtml(string title, string message, string alertType)
        {
            // Get colors based on alert type
            (string bgColor, string textColor, string borderColor) = GetColorsForAlertType(alertType);
            string iconHtml = GetIconHtmlForAlertType(alertType);

            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <meta name='description' content='Payment status notification'>
                    <title>{title}</title>
                    <style>
                        /* Reset and Base Styles */
                        * {{
                            box-sizing: border-box;
                            margin: 0;
                            padding: 0;
                        }}
                        
                        html, body {{
                            height: 100%;
                            width: 100%;
                            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
                            line-height: 1.5;
                        }}
                        
                        body {{
                            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            padding: 20px;
                        }}
                        
                        /* Card Container */
                        .card {{
                            background-color: #ffffff;
                            border-radius: 16px;
                            box-shadow: 0 8px 24px rgba(0, 0, 0, 0.09);
                            max-width: 500px;
                            width: 100%;
                            padding: 40px 25px;
                            text-align: center;
                            transition: transform 0.3s ease, box-shadow 0.3s ease;
                        }}
                        
                        .card:hover {{
                            transform: translateY(-5px);
                            box-shadow: 0 15px 30px rgba(0, 0, 0, 0.12);
                        }}
                        
                        /* Icon Styles */
                        .icon-container {{
                            margin-bottom: 24px;
                        }}
                        
                        .icon-circle {{
                            width: 80px;
                            height: 80px;
                            border-radius: 50%;
                            background-color: {bgColor};
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            margin: 0 auto;
                        }}
                        
                        /* Message Styles */
                        .message-container {{
                            margin-bottom: 24px;
                        }}
                        
                        .heading {{
                            font-weight: 600;
                            margin-bottom: 16px;
                            font-size: 24px;
                            color: #333333;
                        }}
                        
                        .message {{
                            font-size: 16px;
                            line-height: 1.6;
                            color: #555555;
                        }}
                        
                        .close-message {{
                            margin-top: 28px;
                            color: #6c757d;
                            font-size: 14px;
                            font-weight: 300;
                            opacity: 0.8;
                        }}
                        
                        /* Responsive */
                        @media (max-width: 576px) {{
                            .card {{
                                padding: 28px 20px;
                            }}
                            
                            .icon-circle {{
                                width: 60px;
                                height: 60px;
                            }}
                            
                            .heading {{
                                font-size: 20px;
                            }}
                            
                            .message {{
                                font-size: 15px;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='card'>
                        <div class='icon-container'>
                            <div class='icon-circle'>
                                {iconHtml}
                            </div>
                        </div>
                        <div class='message-container'>
                            <h2 class='heading'>{title}</h2>
                            <p class='message'>{message}</p>
                        </div>
                        <div class='close-message'>
                            You may now close this page.
                        </div>
                    </div>
                </body>
                </html>
                ";
        }

        private static (string bgColor, string textColor, string borderColor) GetColorsForAlertType(string alertType)
        {
            return alertType.ToLower() switch
            {
                "success" => ("#d4edda", "#155724", "#c3e6cb"),
                "danger" => ("#f8d7da", "#721c24", "#f5c6cb"),
                "warning" => ("#fff3cd", "#856404", "#ffeeba"),
                "info" => ("#d1ecf1", "#0c5460", "#bee5eb"),
                "secondary" => ("#e2e3e5", "#383d41", "#d6d8db"),
                _ => ("#e2e3e5", "#383d41", "#d6d8db"),
            };
        }

        private static string GetIconHtmlForAlertType(string alertType)
        {
            string iconColor = alertType.ToLower() switch
            {
                "success" => "#28a745",
                "danger" => "#dc3545",
                "warning" => "#ffc107",
                "info" => "#17a2b8",
                "secondary" => "#6c757d",
                _ => "#6c757d",
            };

            return alertType.ToLower() switch
            {
                "success" => $@"<svg xmlns='http://www.w3.org/2000/svg' width='40' height='40' fill='{iconColor}' viewBox='0 0 16 16'>
                                <path d='M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z'/>
                              </svg>",

                "danger" => $@"<svg xmlns='http://www.w3.org/2000/svg' width='40' height='40' fill='{iconColor}' viewBox='0 0 16 16'>
                              <path d='M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z'/>
                            </svg>",

                "secondary" => $@"<svg xmlns='http://www.w3.org/2000/svg' width='40' height='40' fill='{iconColor}' viewBox='0 0 16 16'>
                                <path d='M8 1a2 2 0 0 1 2 2v4H6V3a2 2 0 0 1 2-2zm3 6V3a3 3 0 0 0-6 0v4a2 2 0 0 0-2 2v5a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2z'/>
                              </svg>",

                "warning" => $@"<svg xmlns='http://www.w3.org/2000/svg' width='40' height='40' fill='{iconColor}' viewBox='0 0 16 16'>
                              <path d='M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z'/>
                              <path d='M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z'/>
                            </svg>",

                "info" => $@"<svg xmlns='http://www.w3.org/2000/svg' width='40' height='40' fill='{iconColor}' viewBox='0 0 16 16'>
                          <path d='M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16zm.93-9.412-1 4.705c-.07.34.029.533.304.533.194 0 .487-.07.686-.246l-.088.416c-.287.346-.92.598-1.465.598-.703 0-1.002-.422-.808-1.319l.738-3.468c.064-.293.006-.399-.287-.47l-.451-.081.082-.381 2.29-.287zM8 5.5a1 1 0 1 1 0-2 1 1 0 0 1 0 2z'/>
                        </svg>",

                _ => $@"<svg xmlns='http://www.w3.org/2000/svg' width='40' height='40' fill='{iconColor}' viewBox='0 0 16 16'>
                     <path d='M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z'/>
                     <path d='M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z'/>
                   </svg>"
            };
        }

        // Convenience methods for common scenarios
        public static string GenerateSuccessHtml(string message = "Thank you! Your payment was successful.")
        {
            return GenerateHtml("Payment Successful", message, "success");
        }

        public static string GenerateFailedHtml(string message = "Sorry, your payment could not be completed.")
        {
            return GenerateHtml("Payment Failed", message, "danger");
        }

        public static string GenerateSecurityHtml(string message = "Security validation failed. Please contact support.")
        {
            return GenerateHtml("Invalid HMAC", message, "secondary");
        }
    }
}