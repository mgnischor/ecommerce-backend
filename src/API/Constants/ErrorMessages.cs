namespace ECommerce.API.Constants;

/// <summary>
/// Centralized error messages used across all API controllers
/// </summary>
/// <remarks>
/// This class provides standardized error messages to ensure consistency across the API.
/// All error messages should be referenced from this class to facilitate maintenance and localization.
/// </remarks>
public static class ErrorMessages
{
    #region General Validation

    /// <summary>
    /// Generic message for missing or invalid data
    /// </summary>
    public const string DataRequired = "Data is required";

    /// <summary>
    /// Generic message for ID mismatch between route and body
    /// </summary>
    public const string IdMismatch = "ID mismatch";

    /// <summary>
    /// Generic message for invalid ID format
    /// </summary>
    public const string InvalidId = "Invalid ID";

    #endregion

    #region Pagination

    /// <summary>
    /// Message for invalid page number (must be > 0)
    /// </summary>
    public const string InvalidPageNumber = "Page number must be greater than 0";

    /// <summary>
    /// Message for invalid page size
    /// </summary>
    public const string InvalidPageSize = "Page size must be between 1 and {0}";

    /// <summary>
    /// Message for invalid pagination parameters
    /// </summary>
    public const string InvalidPaginationParameters = "Invalid pagination parameters";

    #endregion

    #region Search and Filters

    /// <summary>
    /// Message when search term is required but not provided
    /// </summary>
    public const string SearchTermRequired = "Search term is required";

    /// <summary>
    /// Message when search term is too short (minimum length)
    /// </summary>
    public const string SearchTermTooShort = "Search term must be at least 2 characters";

    /// <summary>
    /// Message when search term exceeds maximum length
    /// </summary>
    public const string SearchTermTooLong = "Search term must not exceed 100 characters";

    #endregion

    #region Not Found Messages

    /// <summary>
    /// Generic not found message with entity ID
    /// </summary>
    public static string NotFound(string entityType, string id) =>
        $"{entityType} with ID '{id}' not found";

    /// <summary>
    /// Generic not found message without identifier
    /// </summary>
    public static string NotFound(string entityType) => $"{entityType} not found";

    /// <summary>
    /// Generic not found message with entity code
    /// </summary>
    public static string NotFoundByCode(string entityType, string code) =>
        $"{entityType} with code '{code}' not found";

    #endregion

    #region User-specific Messages

    /// <summary>
    /// Message for user data requirement
    /// </summary>
    public const string UserDataRequired = "User data is required";

    /// <summary>
    /// Message when user email already exists
    /// </summary>
    public static string UserEmailAlreadyExists(string email) =>
        $"User with email '{email}' already exists";

    /// <summary>
    /// Message when user not found by ID
    /// </summary>
    public static string UserNotFoundById(string id) => NotFound("User", id);

    /// <summary>
    /// Message when user not found by email
    /// </summary>
    public static string UserNotFoundByEmail(string email) =>
        $"User with email '{email}' not found";

    /// <summary>
    /// Message when user ID not found in authentication token
    /// </summary>
    public const string UserIdNotFoundInToken = "User ID not found in authentication token";

    #endregion

    #region Product-specific Messages

    /// <summary>
    /// Message for product data requirement
    /// </summary>
    public const string ProductDataRequired = "Product data is required";

    /// <summary>
    /// Message when product not found by ID
    /// </summary>
    public static string ProductNotFoundById(string id) => NotFound("Product", id);

    /// <summary>
    /// Message when product not found by SKU
    /// </summary>
    public static string ProductNotFoundBySku(string sku) => NotFoundByCode("Product", sku);

    #endregion

    #region Order-specific Messages

    /// <summary>
    /// Message for order data requirement
    /// </summary>
    public const string OrderDataRequired = "Order data is required";

    /// <summary>
    /// Message when order not found by ID
    /// </summary>
    public static string OrderNotFoundById(string id) => NotFound("Order", id);

    /// <summary>
    /// Message when order item quantity is invalid
    /// </summary>
    public static string InvalidOrderItemQuantity(string productName) =>
        $"Item quantity must be positive for product {productName}";

    #endregion

    #region Vendor-specific Messages

    /// <summary>
    /// Message for vendor data requirement
    /// </summary>
    public const string VendorDataRequired = "Vendor data is required";

    /// <summary>
    /// Message when vendor not found by ID
    /// </summary>
    public static string VendorNotFoundById(string id) => NotFound("Vendor", id);

    #endregion

    #region Supplier-specific Messages

    /// <summary>
    /// Message for supplier data requirement
    /// </summary>
    public const string SupplierDataRequired = "Supplier data is required";

    /// <summary>
    /// Message when supplier not found by ID
    /// </summary>
    public static string SupplierNotFoundById(string id) => NotFound("Supplier", id);

    /// <summary>
    /// Message when supplier not found by code
    /// </summary>
    public static string SupplierNotFoundByCode(string code) => NotFoundByCode("Supplier", code);

    #endregion

    #region Store-specific Messages

    /// <summary>
    /// Message for store data requirement
    /// </summary>
    public const string StoreDataRequired = "Store data is required";

    /// <summary>
    /// Message when store not found by ID
    /// </summary>
    public static string StoreNotFoundById(string id) => NotFound("Store", id);

    /// <summary>
    /// Message when store not found by code
    /// </summary>
    public static string StoreNotFoundByCode(string code) => NotFoundByCode("Store", code);

    /// <summary>
    /// Message when city name is required
    /// </summary>
    public const string CityNameRequired = "City name is required";

    /// <summary>
    /// Message when city name exceeds maximum length
    /// </summary>
    public const string CityNameTooLong = "City name must not exceed 100 characters";

    #endregion

    #region Shipment-specific Messages

    /// <summary>
    /// Message for shipment data requirement
    /// </summary>
    public const string ShipmentDataRequired = "Shipment data is required";

    /// <summary>
    /// Message when shipment not found
    /// </summary>
    public const string ShipmentNotFound = "Shipment not found";

    /// <summary>
    /// Message when shipment not found by ID
    /// </summary>
    public static string ShipmentNotFoundById(string id) => NotFound("Shipment", id);

    /// <summary>
    /// Message when tracking number is required
    /// </summary>
    public const string TrackingNumberRequired = "Tracking number is required";

    /// <summary>
    /// Message when tracking number format is invalid
    /// </summary>
    public const string InvalidTrackingNumberFormat = "Invalid tracking number format";

    /// <summary>
    /// Message when tracking number already exists
    /// </summary>
    public const string TrackingNumberAlreadyExists = "Tracking number already exists";

    /// <summary>
    /// Message when carrier name is required
    /// </summary>
    public const string CarrierNameRequired = "Valid carrier name is required (max 200 characters)";

    /// <summary>
    /// Message when shipping address is required
    /// </summary>
    public const string ShippingAddressRequired =
        "Valid shipping address is required (max 500 characters)";

    /// <summary>
    /// Message when order ID is required for shipment
    /// </summary>
    public const string ShipmentOrderIdRequired = "Valid Order ID is required";

    #endregion

    #region Shipping Zone-specific Messages

    /// <summary>
    /// Message for shipping zone data requirement
    /// </summary>
    public const string ShippingZoneDataRequired = "Shipping zone data is required";

    /// <summary>
    /// Message when shipping zone not found
    /// </summary>
    public const string ShippingZoneNotFound = "Shipping zone not found";

    /// <summary>
    /// Message when shipping zone ID is invalid
    /// </summary>
    public const string InvalidShippingZoneId = "Invalid shipping zone ID";

    /// <summary>
    /// Message when zone name is required
    /// </summary>
    public const string ZoneNameRequired = "Valid zone name is required (max 200 characters)";

    /// <summary>
    /// Message when rates cannot be negative
    /// </summary>
    public const string RatesCannotBeNegative = "Rates cannot be negative";

    /// <summary>
    /// Message when free shipping threshold cannot be negative
    /// </summary>
    public const string FreeShippingThresholdCannotBeNegative =
        "Free shipping threshold cannot be negative";

    /// <summary>
    /// Message when priority must be positive
    /// </summary>
    public const string PriorityMustBePositive = "Priority must be a positive number";

    /// <summary>
    /// Message when shipping zone name already exists
    /// </summary>
    public const string ShippingZoneNameAlreadyExists = "Shipping zone name already exists";

    #endregion

    #region Refund-specific Messages

    /// <summary>
    /// Message for refund data requirement
    /// </summary>
    public const string RefundDataRequired = "Refund data is required";

    /// <summary>
    /// Message when refund not found by ID
    /// </summary>
    public static string RefundNotFoundById(string id) => NotFound("Refund", id);

    /// <summary>
    /// Message when refund not found
    /// </summary>
    public const string RefundNotFound = "Refund not found";

    /// <summary>
    /// Message when refund reason is required
    /// </summary>
    public const string RefundReasonRequired = "Refund reason is required";

    /// <summary>
    /// Message when rejection reason is required
    /// </summary>
    public const string RejectionReasonRequired = "Rejection reason is required";

    /// <summary>
    /// Message when rejection reason exceeds maximum length
    /// </summary>
    public const string RejectionReasonTooLong = "Rejection reason must not exceed 500 characters";

    /// <summary>
    /// Message when refund cannot be rejected in current status
    /// </summary>
    public const string CannotRejectRefundInStatus = "Cannot reject a refund in this status";

    /// <summary>
    /// Message when invalid customer ID
    /// </summary>
    public const string InvalidCustomerId = "Invalid customer ID";

    #endregion

    #region Promotion-specific Messages

    /// <summary>
    /// Message for promotion data requirement
    /// </summary>
    public const string PromotionDataRequired = "Promotion data is required";

    /// <summary>
    /// Message when promotion not found
    /// </summary>
    public const string PromotionNotFound = "Promotion not found";

    /// <summary>
    /// Message when promotion not found by ID
    /// </summary>
    public static string PromotionNotFoundById(string id) => NotFound("Promotion", id);

    /// <summary>
    /// Message when promotion code is required
    /// </summary>
    public const string PromotionCodeRequired = "Promotion code is required";

    /// <summary>
    /// Message when promotion code exceeds maximum length
    /// </summary>
    public const string PromotionCodeTooLong = "Promotion code must not exceed 50 characters";

    /// <summary>
    /// Message when promotion code format is invalid
    /// </summary>
    public const string InvalidPromotionCodeFormat = "Invalid promotion code format";

    #endregion

    #region Conflict Messages

    /// <summary>
    /// Generic conflict message for existing entity
    /// </summary>
    public static string AlreadyExists(string entityType, string identifier, string value) =>
        $"{entityType} with {identifier} '{value}' already exists";

    #endregion

    #region Authentication Messages

    /// <summary>
    /// Message when invalid credentials are provided
    /// </summary>
    public const string InvalidCredentials = "Invalid email or password";

    /// <summary>
    /// Message when user account is banned
    /// </summary>
    public const string AccountBanned = "Your account has been banned";

    /// <summary>
    /// Message when user account is deleted
    /// </summary>
    public const string AccountDeleted = "This account has been deleted";

    /// <summary>
    /// Message when user account is inactive
    /// </summary>
    public const string AccountInactive = "Your account is inactive";

    #endregion
}
