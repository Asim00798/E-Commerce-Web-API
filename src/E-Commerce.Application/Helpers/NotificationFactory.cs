using E_Commerce.Domain.Enums;
using E_Commerce.Domain.Responses;

namespace E_Commerce.Domain.Helpers
{
    public static class NotificationFactory
    {
        public static NotificationResponse OrderPlaced(string userId, string orderId)
        {
            return new NotificationResponse
            {
                ReceiverId = userId,
                ReceiverType = NotificationReceiverType.Customer,
                Message = $"The order #{orderId} has been placed successfully.",
                IsRead = false
            };
        }

        public static NotificationResponse PaymentSucceeded(string userId, string orderId)
        {
            return new NotificationResponse
            {
                ReceiverId = userId,
                ReceiverType = NotificationReceiverType.Customer,
                Message = $"Payment succeeded for order #{orderId}. We are pleased to deal with you.",
                IsRead = false
            };
        }

        public static NotificationResponse PaymentFailed(string userId, string orderId)
        {
            return new NotificationResponse
            {
                ReceiverId = userId,
                ReceiverType = NotificationReceiverType.Customer,
                Message = $"Payment failed for order #{orderId}. Try again.",
                IsRead = false
            };
        }

        public static NotificationResponse OrderShipped(string userId, string orderId, string trackingNumber)
        {
            return new NotificationResponse
            {
                ReceiverId = userId,
                ReceiverType = NotificationReceiverType.Customer,
                Message = $"Your order #{orderId} has been shipped successfully. We are pleased to deal with you.",
                IsRead = false
            };
        }
    }
}
