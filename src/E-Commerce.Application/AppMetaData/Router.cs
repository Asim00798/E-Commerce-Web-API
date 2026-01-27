namespace E_Commerce.Domain.AppMetaData
{
    public static class Router
    {
        public const string SingleRoute = "{id}";

        public const string Root = "api";
        public const string Version = "v1";
        public const string Rule = Root + "/" + Version + "/";

        public static class EmailsRoute
        {
            public const string Prefix = Rule + "email/";
            public const string SendEmail = Prefix + "sendEmail";
        }

        public static class Authentication
        {
            public const string Prefix = Rule + "authenticate/";
            public const string SignIn = Prefix + "signIn";
            public const string SignInViaGoogle = Prefix + "signInViaGoogle";
            public const string RefreshToken = Prefix + "refreshToken";
            public const string ValidateToken = Prefix + "validateToken";
            public const string SendResetPasswordCode = Prefix + "sendResetPasswordCode";
            public const string ConfirmResetPasswordCode = Prefix + "confirmResetPasswordCode";
            public const string ResetPassword = Prefix + "resetPassword";
            public const string ConfirmEmail = "api/authenticate/confirmEmail";
        }

        public static class Authorization
        {
            public const string Prefix = Rule + "authorize/";
            public const string Role = Prefix + "role/";
            public const string Claim = Prefix + "claim/";
            public const string CreateRole = Role + "create";
            public const string EditRole = Role + "edit";
            public const string DeleteRole = Role + SingleRoute;
            public const string GetAllRoles = Role + "getAll";
            public const string GetRoleById = Role + SingleRoute;
            public const string ManageUserRoles = Role + "manageUserRoles/" + SingleRoute;
            public const string UpdateUserRoles = Role + "updateUserRoles";
            public const string ManageUserClaims = Claim + "manageUserClaims/" + SingleRoute;
            public const string UpdateUserClaims = Claim + "updateUserClaims";
        }

        public static class UserRouting
        {
            public const string Prefix = Rule + "user/";
            public const string Register = Prefix + "register";
            public const string ChangePassword = Prefix + "changePassword";
        }

        public static class CategoryRouting
        {
            public const string Prefix = Rule + "category/";
            public const string GetAll = Prefix + "getAll";
            public const string Paginated = Prefix + "paginated";
            public const string GetById = Prefix + SingleRoute;
            public const string Create = Prefix + "create";
            public const string Edit = Prefix + "edit";
            public const string Delete = Prefix + SingleRoute;
        }

        public static class ProductRouting
        {
            public const string Prefix = Rule + "product/";
            public const string Paginated = Prefix + "paginated";
            public const string GetSingle = Prefix + "getSingle";
            public const string Create = Prefix + "create";
            public const string Edit = Prefix + "edit";
            public const string Delete = Prefix + SingleRoute;
        }

        public static class OrderRouting
        {
            public const string Prefix = Rule + "order/";
            public const string GetMyOrders = Prefix + "getMyOrders";
            public const string Paginated = Prefix + "paginated";
            public const string GetById = Prefix + SingleRoute;
            public const string Create = Prefix + "create";
            public const string Delete = Prefix + SingleRoute;
            public const string PlaceOrder = Prefix + "placeOrder/" + SingleRoute;
        }

        public static class CartRouting
        {
            public const string Prefix = Rule + "cart/";
            public const string GetById = Prefix + SingleRoute;
            public const string GetMyCart = Prefix + "myCart";
            public const string AddToCart = Prefix + "addToCart";
            public const string UpdateItemQuantity = Prefix + "updateItemQuantity";
            public const string RemoveFromCart = Prefix + "removeFromCart/" + SingleRoute;
            public const string Delete = Prefix + SingleRoute;
        }

        public static class CustomerRouting
        {
            public const string Prefix = Rule + "customer/";
            public const string Paginated = Prefix + "paginated";
            public const string GetById = Prefix + SingleRoute;
            public const string Edit = Prefix + "edit";
            public const string Delete = Prefix + SingleRoute;
        }

        public static class EmployeeRouting
        {
            public const string Prefix = Rule + "employee/";
            public const string GetAll = Prefix + "getAll";
            public const string Paginated = Prefix + "paginated";
            public const string GetById = Prefix + SingleRoute;
            public const string Create = Prefix + "create";
            public const string Edit = Prefix + "edit";
            public const string Delete = Prefix + SingleRoute;
        }

        public static class PaymentRouting
        {
            public const string Prefix = Rule + "payment/";
            public const string SetPaymentMethod = Prefix + "setPaymentMethod";
            public const string ServerCallback = Prefix + "serverCallback";
            public const string PaymobCallback = Prefix + "paymobCallback";
        }

        public static class DeliveryRouting
        {
            public const string Prefix = Rule + "delivery/";
            public const string SetDeliveryMethod = Prefix + "setDeliveryMethod";
            public const string EditDeliveryMethod = Prefix + "editDeliveryMethod";
        }

        public static class ShippingAddressRouting
        {
            public const string Prefix = Rule + "shippingAddress/";
            public const string GetAll = Prefix + "getAll";
            public const string GetById = Prefix + SingleRoute;
            public const string Create = Prefix + "create";
            public const string Edit = Prefix + "edit";
            public const string Delete = Prefix + SingleRoute;
            public const string SetShippingAddress = Prefix + "setShippingAddress";
        }

        public static class ReviewRouting
        {
            public const string Prefix = Rule + "review/";
            public const string Paginated = Prefix + "paginated";
            public const string Create = Prefix + "create";
            public const string Edit = Prefix + "edit";
            public const string Delete = Prefix + SingleRoute;
        }

        public static class NotificationsRouting
        {
            public const string Prefix = Rule + "notifications/";
            public const string Paginated = Prefix + "paginated";
            public const string MarkAsRead = Prefix + "markAsRead/" + SingleRoute;
            public const string MarkAllAsRead = Prefix + "markAllAsRead";
        }
    }
}
