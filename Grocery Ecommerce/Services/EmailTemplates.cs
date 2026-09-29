namespace Grocery_Ecommerce.Services
{
    public static class EmailTemplates
    {
        public static string WelcomeEmailTemplate(string fullName, string email)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background: #f9fafb; border-radius: 8px; }}
        .header {{ background: linear-gradient(135deg, #27ae60 0%, #229954 100%); color: white; padding: 30px; border-radius: 8px 8px 0 0; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ background: white; padding: 30px; border-radius: 0 0 8px 8px; }}
        .greeting {{ font-size: 18px; font-weight: 600; color: #1f2937; margin-bottom: 15px; }}
        .message {{ color: #6b7280; margin-bottom: 20px; }}
        .features {{ background: #f0f8f5; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #27ae60; }}
        .features h3 {{ color: #27ae60; margin-top: 0; }}
        .features ul {{ margin: 10px 0; padding-left: 20px; }}
        .features li {{ margin: 8px 0; color: #374151; }}
        .footer {{ text-align: center; padding: 20px; color: #9ca3af; font-size: 12px; border-top: 1px solid #e5e7eb; }}
        .button {{ display: inline-block; background: #27ae60; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; margin: 20px 0; font-weight: 600; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Grocery Store!</h1>
        </div>
        <div class='content'>
            <div class='greeting'>Hello {fullName}! ??</div>
            <div class='message'>
                Thank you for creating an account with us. We're excited to have you join our community of fresh grocery enthusiasts!
            </div>
            
            <div class='features'>
                <h3>What You Can Do Now:</h3>
                <ul>
                    <li>-> Browse our fresh collection of quality groceries</li>
                    <li>-> Add items to your cart and checkout easily</li>
                    <li>-> Track your orders in real-time</li>
                    <li>-> Save your favorite products for quick access</li>
                    <li>-> Enjoy free delivery on orders above 50TK</li>
                </ul>
            </div>

            <div class='message'>
                <strong>Your Account Details:</strong><br>
                Email: {email}<br>
                Account Created: {DateTime.Now:MMMM dd, yyyy}
            </div>

            <div style='text-align: center;'>
                <a href='https://localhost:7006/' class='button'>Start Shopping Now</a>
            </div>

            <div class='message'>
                If you have any questions or need assistance, our customer support team is here to help 24/7.
            </div>
        </div>
        <div class='footer'>
            <p>© 2024 Grocery Store. All rights reserved.<br>
            This email was sent to {email}<br>
            If you didn't create this account, please ignore this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string OrderConfirmationTemplate(int orderId, decimal totalAmount, List<string> items, string fullName)
        {
            var itemsList = string.Join("<li>", items);

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background: #f9fafb; border-radius: 8px; }}
        .header {{ background: linear-gradient(135deg, #27ae60 0%, #229954 100%); color: white; padding: 30px; border-radius: 8px 8px 0 0; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ background: white; padding: 30px; border-radius: 0 0 8px 8px; }}
        .order-info {{ background: #f0f8f5; padding: 20px; border-radius: 8px; border-left: 4px solid #27ae60; margin: 20px 0; }}
        .order-number {{ font-size: 24px; font-weight: 700; color: #27ae60; }}
        .order-date {{ color: #6b7280; margin-top: 5px; }}
        .items-section {{ margin: 25px 0; }}
        .items-section h3 {{ color: #1f2937; border-bottom: 2px solid #e5e7eb; padding-bottom: 10px; }}
        .items-section ul {{ list-style-type: none; padding: 0; }}
        .items-section li {{ padding: 8px 0; color: #374151; border-bottom: 1px solid #f3f4f6; }}
        .summary {{ background: #fef3c7; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #f59e0b; }}
        .summary-row {{ display: flex; justify-content: space-between; padding: 8px 0; }}
        .summary-row.total {{ font-size: 18px; font-weight: 700; color: #27ae60; border-top: 2px solid #fcd34d; padding-top: 12px; }}
        .button {{ display: inline-block; background: #27ae60; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; margin: 20px 0; font-weight: 600; }}
        .footer {{ text-align: center; padding: 20px; color: #9ca3af; font-size: 12px; border-top: 1px solid #e5e7eb; }}
        .status {{ background: #d1fae5; color: #065f46; padding: 12px; border-radius: 6px; text-align: center; font-weight: 600; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Confirmed!</h1>
        </div>
        <div class='content'>
            <p style='font-size: 16px; color: #6b7280;'>Hi {fullName},</p>
            
            <p style='color: #6b7280;'>Thank you for your order! We've received your purchase and it's being prepared for delivery.</p>

            <div class='order-info'>
                <div class='order-number'>Order #{orderId}</div>
                <div class='order-date'>Ordered on: {DateTime.Now:MMMM dd, yyyy at h:mm tt}</div>
            </div>

            <div class='status'>
                 Order Status: Confirmed & Processing
            </div>

            <div class='items-section'>
                <h3>Order Items:</h3>
                <ul>
                    <li>{itemsList}</li>
                </ul>
            </div>

            <div class='summary'>
                <div class='summary-row'>
                    <span>Subtotal:</span>
                    <span>{(totalAmount / 1.05m):F2}Tk</span>
                </div>
                <div class='summary-row'>
                    <span>Tax (5%):</span>
                    <span>{(totalAmount / 1.05m * 0.05m):F2}Tk</span>
                </div>
                <div class='summary-row'>
                    <span>Delivery:</span>
                    <span style='color: #27ae60;'><strong>FREE</strong></span>
                </div>
                <div class='summary-row total'>
                    <span>Total Amount:</span>
                    <span>{totalAmount:F2}Tk</span>
                </div>
            </div>

            <div style='background: #eff6ff; padding: 15px; border-radius: 6px; border-left: 4px solid #667eea; margin: 20px 0;'>
                <h4 style='margin-top: 0; color: #1e40af;'>?? What's Next?</h4>
                <ul style='margin: 0; padding-left: 20px; color: #1e40af;'>
                    <li>Your order will be picked and packed within 1-2 hours</li>
                    <li>You'll receive a delivery update once it's on the way</li>
                    <li>Expected delivery: Within 24 hours</li>
                </ul>
            </div>

            <div style='text-align: center;'>
                <a href='https://localhost:7006/Orders' class='button'>Track Your Order</a>
            </div>

            <div class='message'>
                <p style='color: #6b7280;'>
                    Questions about your order? Our customer support team is available 24/7 to help.<br>
                    <strong>Email:</strong> support@grocerystore.com<br>
                    <strong>Phone:</strong> +880123-456-7890
                </p>
            </div>
        </div>
        <div class='footer'>
            <p>© 2024 Grocery Store. All rights reserved.<br>
            You received this email because you have an account with us.<br>
            <a href='https://localhost:7006/' style='color: #667eea; text-decoration: none;'>Visit our website</a></p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
