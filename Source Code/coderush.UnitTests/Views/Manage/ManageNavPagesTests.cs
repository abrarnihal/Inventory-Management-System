using System;
using System.IO;

using coderush.Views.Manage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Views.Manage.UnitTests
{
    /// <summary>
    /// Unit tests for the ManageNavPages static class.
    /// </summary>
    [TestClass]
    public class ManageNavPagesTests
    {
        /// <summary>
        /// Tests that ActivePageKey returns the expected constant value "ActivePage".
        /// </summary>
        [TestMethod]
        public void ActivePageKey_WhenAccessed_ReturnsActivePageString()
        {
            // Arrange & Act
            var result = ManageNavPages.ActivePageKey;

            // Assert
            Assert.AreEqual("ActivePage", result);
        }

        /// <summary>
        /// Tests that ActivePageKey returns a non-null value.
        /// </summary>
        [TestMethod]
        public void ActivePageKey_WhenAccessed_ReturnsNonNullValue()
        {
            // Arrange & Act
            var result = ManageNavPages.ActivePageKey;

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests that ActivePageKey returns a non-empty string.
        /// </summary>
        [TestMethod]
        public void ActivePageKey_WhenAccessed_ReturnsNonEmptyString()
        {
            // Arrange & Act
            var result = ManageNavPages.ActivePageKey;

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }

        /// <summary>
        /// Tests that ActivePageKey returns the same value on multiple accesses.
        /// </summary>
        [TestMethod]
        public void ActivePageKey_WhenAccessedMultipleTimes_ReturnsConsistentValue()
        {
            // Arrange & Act
            var result1 = ManageNavPages.ActivePageKey;
            var result2 = ManageNavPages.ActivePageKey;

            // Assert
            Assert.AreEqual(result1, result2);
        }

        /// <summary>
        /// Tests that AddActivePage sets the active page value in ViewData with a normal string value.
        /// Input: Valid ViewDataDictionary and normal string "TestPage".
        /// Expected: ViewData contains "ActivePage" key with value "TestPage".
        /// </summary>
        [TestMethod]
        public void AddActivePage_NormalString_SetsValueInViewData()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            string activePage = "TestPage";

            // Act
            global::coderush.Views.Manage.ManageNavPages.AddActivePage(viewData, activePage);

            // Assert
            Assert.IsTrue(viewData.ContainsKey("ActivePage"));
            Assert.AreEqual("TestPage", viewData["ActivePage"]);
        }

        /// <summary>
        /// Tests that AddActivePage accepts null as the active page value.
        /// Input: Valid ViewDataDictionary and null activePage.
        /// Expected: ViewData contains "ActivePage" key with null value.
        /// </summary>
        [TestMethod]
        public void AddActivePage_NullActivePage_SetsNullInViewData()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            string? activePage = null;

            // Act
            global::coderush.Views.Manage.ManageNavPages.AddActivePage(viewData, activePage!);

            // Assert
            Assert.IsTrue(viewData.ContainsKey("ActivePage"));
            Assert.IsNull(viewData["ActivePage"]);
        }

        /// <summary>
        /// Tests that AddActivePage accepts empty string as the active page value.
        /// Input: Valid ViewDataDictionary and empty string activePage.
        /// Expected: ViewData contains "ActivePage" key with empty string value.
        /// </summary>
        [TestMethod]
        public void AddActivePage_EmptyString_SetsEmptyStringInViewData()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            string activePage = string.Empty;

            // Act
            global::coderush.Views.Manage.ManageNavPages.AddActivePage(viewData, activePage);

            // Assert
            Assert.IsTrue(viewData.ContainsKey("ActivePage"));
            Assert.AreEqual(string.Empty, viewData["ActivePage"]);
        }

        /// <summary>
        /// Tests that AddActivePage accepts whitespace-only string as the active page value.
        /// Input: Valid ViewDataDictionary and whitespace-only string activePage.
        /// Expected: ViewData contains "ActivePage" key with whitespace string value.
        /// </summary>
        [TestMethod]
        public void AddActivePage_WhitespaceString_SetsWhitespaceInViewData()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            string activePage = "   ";

            // Act
            global::coderush.Views.Manage.ManageNavPages.AddActivePage(viewData, activePage);

            // Assert
            Assert.IsTrue(viewData.ContainsKey("ActivePage"));
            Assert.AreEqual("   ", viewData["ActivePage"]);
        }

        /// <summary>
        /// Tests that AddActivePage accepts very long strings as the active page value.
        /// Input: Valid ViewDataDictionary and very long string activePage.
        /// Expected: ViewData contains "ActivePage" key with the long string value.
        /// </summary>
        [TestMethod]
        public void AddActivePage_VeryLongString_SetsLongStringInViewData()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            string activePage = new string('a', 10000);

            // Act
            global::coderush.Views.Manage.ManageNavPages.AddActivePage(viewData, activePage);

            // Assert
            Assert.IsTrue(viewData.ContainsKey("ActivePage"));
            Assert.AreEqual(activePage, viewData["ActivePage"]);
        }

        /// <summary>
        /// Tests that AddActivePage accepts strings with special characters as the active page value.
        /// Input: Valid ViewDataDictionary and string with special characters.
        /// Expected: ViewData contains "ActivePage" key with the special character string value.
        /// </summary>
        [TestMethod]
        public void AddActivePage_SpecialCharacters_SetsSpecialCharactersInViewData()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            string activePage = "!@#$%^&*()_+-=[]{}|;':\",./<>?\\`~";

            // Act
            global::coderush.Views.Manage.ManageNavPages.AddActivePage(viewData, activePage);

            // Assert
            Assert.IsTrue(viewData.ContainsKey("ActivePage"));
            Assert.AreEqual(activePage, viewData["ActivePage"]);
        }

        /// <summary>
        /// Tests that AddActivePage overwrites existing "ActivePage" value in ViewData.
        /// Input: ViewDataDictionary with existing "ActivePage" key, and new activePage value.
        /// Expected: ViewData "ActivePage" key is updated with the new value.
        /// </summary>
        [TestMethod]
        public void AddActivePage_ExistingActivePageKey_OverwritesValue()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            viewData["ActivePage"] = "OldPage";
            string activePage = "NewPage";

            // Act
            global::coderush.Views.Manage.ManageNavPages.AddActivePage(viewData, activePage);

            // Assert
            Assert.AreEqual("NewPage", viewData["ActivePage"]);
        }

        /// <summary>
        /// Tests that AddActivePage preserves other keys in ViewData when adding "ActivePage".
        /// Input: ViewDataDictionary with existing keys, and new activePage value.
        /// Expected: ViewData contains both existing keys and new "ActivePage" key.
        /// </summary>
        [TestMethod]
        public void AddActivePage_ViewDataWithOtherKeys_PreservesOtherKeys()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            viewData["OtherKey1"] = "Value1";
            viewData["OtherKey2"] = "Value2";
            string activePage = "TestPage";

            // Act
            global::coderush.Views.Manage.ManageNavPages.AddActivePage(viewData, activePage);

            // Assert
            Assert.AreEqual(3, viewData.Count);
            Assert.AreEqual("Value1", viewData["OtherKey1"]);
            Assert.AreEqual("Value2", viewData["OtherKey2"]);
            Assert.AreEqual("TestPage", viewData["ActivePage"]);
        }

        /// <summary>
        /// Tests that AddActivePage accepts strings with control characters as the active page value.
        /// Input: Valid ViewDataDictionary and string with control characters.
        /// Expected: ViewData contains "ActivePage" key with the control character string value.
        /// </summary>
        [TestMethod]
        public void AddActivePage_ControlCharacters_SetsControlCharactersInViewData()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            string activePage = "Page\t\n\r\0Name";

            // Act
            global::coderush.Views.Manage.ManageNavPages.AddActivePage(viewData, activePage);

            // Assert
            Assert.IsTrue(viewData.ContainsKey("ActivePage"));
            Assert.AreEqual(activePage, viewData["ActivePage"]);
        }

        /// <summary>
        /// Tests that AddActivePage accepts Unicode characters as the active page value.
        /// Input: Valid ViewDataDictionary and string with Unicode characters.
        /// Expected: ViewData contains "ActivePage" key with the Unicode string value.
        /// </summary>
        [TestMethod]
        public void AddActivePage_UnicodeCharacters_SetsUnicodeInViewData()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            string activePage = "页面名称🎉";

            // Act
            global::coderush.Views.Manage.ManageNavPages.AddActivePage(viewData, activePage);

            // Assert
            Assert.IsTrue(viewData.ContainsKey("ActivePage"));
            Assert.AreEqual(activePage, viewData["ActivePage"]);
        }

        /// <summary>
        /// Tests that the ChangePassword property returns the expected constant string value "ChangePassword".
        /// </summary>
        [TestMethod]
        public void ChangePassword_WhenAccessed_ReturnsChangePasswordString()
        {
            // Arrange
            const string expected = "ChangePassword";

            // Act
            string actual = ManageNavPages.ChangePassword;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests that the ChangePassword property does not return null.
        /// </summary>
        [TestMethod]
        public void ChangePassword_WhenAccessed_ReturnsNonNullValue()
        {
            // Act
            string actual = ManageNavPages.ChangePassword;

            // Assert
            Assert.IsNotNull(actual);
        }

        /// <summary>
        /// Tests that the ChangePassword property does not return an empty or whitespace string.
        /// </summary>
        [TestMethod]
        public void ChangePassword_WhenAccessed_ReturnsNonEmptyString()
        {
            // Act
            string actual = ManageNavPages.ChangePassword;

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(actual));
        }

        /// <summary>
        /// Tests that the ChangePassword property returns the same value consistently across multiple accesses.
        /// </summary>
        [TestMethod]
        public void ChangePassword_WhenAccessedMultipleTimes_ReturnsConsistentValue()
        {
            // Act
            string firstAccess = ManageNavPages.ChangePassword;
            string secondAccess = ManageNavPages.ChangePassword;

            // Assert
            Assert.AreEqual(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that PageNavClass returns "active" when the active page matches the provided page name (exact case).
        /// Input: ViewData["ActivePage"] = "Index", page = "Index"
        /// Expected: Returns "active"
        /// </summary>
        [TestMethod]
        public void PageNavClass_ActivePageMatchesExactCase_ReturnsActive()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "Index";

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "Index");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that PageNavClass returns "active" when the active page matches the provided page name with different casing.
        /// Input: ViewData["ActivePage"] = "Index", page = "index"
        /// Expected: Returns "active" (case-insensitive comparison)
        /// </summary>
        [TestMethod]
        [DataRow("Index", "index")]
        [DataRow("Index", "INDEX")]
        [DataRow("Index", "InDeX")]
        [DataRow("ChangePassword", "changepassword")]
        [DataRow("ExternalLogins", "EXTERNALLOGINS")]
        public void PageNavClass_ActivePageMatchesDifferentCase_ReturnsActive(string activePageValue, string pageParameter)
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = activePageValue;

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, pageParameter);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that PageNavClass returns null when the active page does not match the provided page name.
        /// Input: ViewData["ActivePage"] = "Index", page = "ChangePassword"
        /// Expected: Returns null
        /// </summary>
        [TestMethod]
        [DataRow("Index", "ChangePassword")]
        [DataRow("Index", "ExternalLogins")]
        [DataRow("ChangePassword", "Index")]
        [DataRow("Index", "Different")]
        public void PageNavClass_ActivePageDoesNotMatch_ReturnsNull(string activePageValue, string pageParameter)
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = activePageValue;

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, pageParameter);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that PageNavClass returns "active" when both ViewData["ActivePage"] and page parameter are null.
        /// Input: ViewData["ActivePage"] = null, page = null
        /// Expected: Returns "active" (null equals null)
        /// </summary>
        [TestMethod]
        public void PageNavClass_BothActivePageAndPageAreNull_ReturnsActive()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = null;

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, null);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that PageNavClass returns null when ViewData["ActivePage"] is null but page parameter is not null.
        /// Input: ViewData["ActivePage"] = null, page = "Index"
        /// Expected: Returns null
        /// </summary>
        [TestMethod]
        public void PageNavClass_ActivePageNullAndPageNotNull_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = null;

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "Index");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that PageNavClass returns null when ViewData["ActivePage"] exists but page parameter is null.
        /// Input: ViewData["ActivePage"] = "Index", page = null
        /// Expected: Returns null
        /// </summary>
        [TestMethod]
        public void PageNavClass_ActivePageNotNullAndPageNull_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "Index";

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, null);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that PageNavClass returns null when ViewData["ActivePage"] key does not exist.
        /// Input: ViewData does not contain "ActivePage" key, page = "Index"
        /// Expected: Returns null (missing key returns null)
        /// </summary>
        [TestMethod]
        public void PageNavClass_ActivePageKeyDoesNotExist_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "Index");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that PageNavClass returns null when ViewData["ActivePage"] contains a non-string value.
        /// Input: ViewData["ActivePage"] = 123 (int), page = "Index"
        /// Expected: Returns null (cast to string fails, activePage becomes null)
        /// </summary>
        [TestMethod]
        [DataRow(123)]
        [DataRow(true)]
        public void PageNavClass_ActivePageIsNonStringValue_ReturnsNull(object nonStringValue)
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = nonStringValue;

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "Index");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that PageNavClass returns "active" when both ViewData["ActivePage"] and page parameter are empty strings.
        /// Input: ViewData["ActivePage"] = "", page = ""
        /// Expected: Returns "active" (empty string equals empty string)
        /// </summary>
        [TestMethod]
        public void PageNavClass_BothActivePageAndPageAreEmptyStrings_ReturnsActive()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = string.Empty;

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, string.Empty);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that PageNavClass returns null when ViewData["ActivePage"] is empty but page parameter is not.
        /// Input: ViewData["ActivePage"] = "", page = "Index"
        /// Expected: Returns null
        /// </summary>
        [TestMethod]
        public void PageNavClass_ActivePageEmptyAndPageNotEmpty_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = string.Empty;

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "Index");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that PageNavClass returns "active" when both ViewData["ActivePage"] and page parameter are whitespace strings.
        /// Input: ViewData["ActivePage"] = "  ", page = "  "
        /// Expected: Returns "active" (same whitespace)
        /// </summary>
        [TestMethod]
        public void PageNavClass_BothActivePageAndPageAreSameWhitespace_ReturnsActive()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "  ";

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "  ");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that PageNavClass returns null when ViewData["ActivePage"] and page parameter are different whitespace strings.
        /// Input: ViewData["ActivePage"] = "  ", page = "   "
        /// Expected: Returns null (different length whitespace)
        /// </summary>
        [TestMethod]
        public void PageNavClass_ActivePageAndPageAreDifferentWhitespace_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "  ";

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "   ");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that PageNavClass returns null when ViewData["ActivePage"] is whitespace but page parameter is not.
        /// Input: ViewData["ActivePage"] = "  ", page = "Index"
        /// Expected: Returns null
        /// </summary>
        [TestMethod]
        public void PageNavClass_ActivePageWhitespaceAndPageNotWhitespace_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "  ";

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "Index");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that the Index property returns the expected constant value "Index".
        /// </summary>
        [TestMethod]
        public void Index_WhenAccessed_ReturnsIndexString()
        {
            // Act
            string result = ManageNavPages.Index;

            // Assert
            Assert.AreEqual("Index", result);
        }

        /// <summary>
        /// Tests that the Index property returns a non-null and non-empty value.
        /// </summary>
        [TestMethod]
        public void Index_WhenAccessed_ReturnsNonNullAndNonEmptyValue()
        {
            // Act
            string result = ManageNavPages.Index;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }

        /// <summary>
        /// Tests that the Index property returns a consistent value across multiple accesses.
        /// </summary>
        [TestMethod]
        public void Index_WhenAccessedMultipleTimes_ReturnsConsistentValue()
        {
            // Act
            string result1 = ManageNavPages.Index;
            string result2 = ManageNavPages.Index;

            // Assert
            Assert.AreEqual(result1, result2);
        }

        /// <summary>
        /// Tests that the TwoFactorAuthentication property returns the expected constant string value.
        /// </summary>
        [TestMethod]
        public void TwoFactorAuthentication_WhenAccessed_ReturnsExpectedValue()
        {
            // Arrange
            const string expected = "TwoFactorAuthentication";

            // Act
            string actual = ManageNavPages.TwoFactorAuthentication;

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.IsNotNull(actual);
            Assert.IsFalse(string.IsNullOrEmpty(actual));
        }

        /// <summary>
        /// Tests that the TwoFactorAuthentication property returns a consistent value across multiple accesses.
        /// </summary>
        [TestMethod]
        public void TwoFactorAuthentication_MultipleAccesses_ReturnsSameValue()
        {
            // Arrange & Act
            string firstAccess = ManageNavPages.TwoFactorAuthentication;
            string secondAccess = ManageNavPages.TwoFactorAuthentication;

            // Assert
            Assert.AreEqual(firstAccess, secondAccess);
            Assert.AreSame(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that TwoFactorAuthenticationNavClass returns "active" when ActivePage matches "TwoFactorAuthentication" with exact case.
        /// </summary>
        [TestMethod]
        public void TwoFactorAuthenticationNavClass_ActivePageMatchesExactCase_ReturnsActive()
        {
            // Arrange
            ViewContext viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "TwoFactorAuthentication";

            // Act
            string result = ManageNavPages.TwoFactorAuthenticationNavClass(viewContext);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that TwoFactorAuthenticationNavClass returns "active" when ActivePage matches "TwoFactorAuthentication" with different case (case-insensitive comparison).
        /// </summary>
        [TestMethod]
        [DataRow("twofactorauthentication")]
        [DataRow("TWOFACTORAUTHENTICATION")]
        [DataRow("TwoFactorAuthentication")]
        [DataRow("tWoFaCtOrAuThEnTiCaTiOn")]
        public void TwoFactorAuthenticationNavClass_ActivePageMatchesDifferentCase_ReturnsActive(string activePage)
        {
            // Arrange
            ViewContext viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = activePage;

            // Act
            string result = ManageNavPages.TwoFactorAuthenticationNavClass(viewContext);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that TwoFactorAuthenticationNavClass returns null when ActivePage is a different page name.
        /// </summary>
        [TestMethod]
        [DataRow("Index")]
        [DataRow("ChangePassword")]
        [DataRow("ExternalLogins")]
        [DataRow("SomeOtherPage")]
        public void TwoFactorAuthenticationNavClass_ActivePageIsDifferent_ReturnsNull(string activePage)
        {
            // Arrange
            ViewContext viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = activePage;

            // Act
            string result = ManageNavPages.TwoFactorAuthenticationNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that TwoFactorAuthenticationNavClass returns null when ActivePage is null.
        /// </summary>
        [TestMethod]
        public void TwoFactorAuthenticationNavClass_ActivePageIsNull_ReturnsNull()
        {
            // Arrange
            ViewContext viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = null;

            // Act
            string result = ManageNavPages.TwoFactorAuthenticationNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that TwoFactorAuthenticationNavClass returns null when ActivePage is not set in ViewData.
        /// </summary>
        [TestMethod]
        public void TwoFactorAuthenticationNavClass_ActivePageNotSet_ReturnsNull()
        {
            // Arrange
            ViewContext viewContext = CreateViewContext();

            // Act
            string result = ManageNavPages.TwoFactorAuthenticationNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that TwoFactorAuthenticationNavClass returns null when ActivePage is not a string type.
        /// </summary>
        [TestMethod]
        public void TwoFactorAuthenticationNavClass_ActivePageIsNotString_ReturnsNull()
        {
            // Arrange
            ViewContext viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = 123;

            // Act
            string result = ManageNavPages.TwoFactorAuthenticationNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that TwoFactorAuthenticationNavClass returns null when ActivePage is an empty string.
        /// </summary>
        [TestMethod]
        public void TwoFactorAuthenticationNavClass_ActivePageIsEmpty_ReturnsNull()
        {
            // Arrange
            ViewContext viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = string.Empty;

            // Act
            string result = ManageNavPages.TwoFactorAuthenticationNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that TwoFactorAuthenticationNavClass returns null when ActivePage is whitespace.
        /// </summary>
        [TestMethod]
        [DataRow(" ")]
        [DataRow("   ")]
        [DataRow("\t")]
        [DataRow("\n")]
        public void TwoFactorAuthenticationNavClass_ActivePageIsWhitespace_ReturnsNull(string activePage)
        {
            // Arrange
            ViewContext viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = activePage;

            // Act
            string result = ManageNavPages.TwoFactorAuthenticationNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Helper method to create a ViewContext instance for testing.
        /// </summary>
        private ViewContext CreateViewContext()
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            var writer = new StringWriter();
            var viewMock = Mock.Of<IView>();

            return new ViewContext(
                actionContext,
                viewMock,
                viewData,
                tempData,
                writer,
                new HtmlHelperOptions());
        }

        /// <summary>
        /// Tests that IndexNavClass returns "active" when ViewData["ActivePage"] equals "Index" (exact match).
        /// </summary>
        [TestMethod]
        public void IndexNavClass_ActivePageEqualsIndex_ReturnsActive()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "Index";

            // Act
            var result = ManageNavPages.IndexNavClass(viewContext);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IndexNavClass returns "active" when ViewData["ActivePage"] equals "index" (case-insensitive match).
        /// </summary>
        [TestMethod]
        public void IndexNavClass_ActivePageEqualsIndexLowerCase_ReturnsActive()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "index";

            // Act
            var result = ManageNavPages.IndexNavClass(viewContext);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IndexNavClass returns "active" when ViewData["ActivePage"] equals "INDEX" (case-insensitive match).
        /// </summary>
        [TestMethod]
        public void IndexNavClass_ActivePageEqualsIndexUpperCase_ReturnsActive()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "INDEX";

            // Act
            var result = ManageNavPages.IndexNavClass(viewContext);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IndexNavClass returns null when ViewData["ActivePage"] is not set.
        /// </summary>
        [TestMethod]
        public void IndexNavClass_ActivePageNotSet_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();

            // Act
            var result = ManageNavPages.IndexNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that IndexNavClass returns null when ViewData["ActivePage"] is a different page name.
        /// </summary>
        [TestMethod]
        [DataRow("ChangePassword")]
        [DataRow("ExternalLogins")]
        [DataRow("TwoFactorAuthentication")]
        [DataRow("SomethingElse")]
        [DataRow("")]
        [DataRow(" ")]
        public void IndexNavClass_ActivePageIsDifferent_ReturnsNull(string activePage)
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = activePage;

            // Act
            var result = ManageNavPages.IndexNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that IndexNavClass returns null when ViewData["ActivePage"] is not a string type.
        /// </summary>
        [TestMethod]
        public void IndexNavClass_ActivePageIsNonString_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = 123;

            // Act
            var result = ManageNavPages.IndexNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that IndexNavClass returns null when ViewData["ActivePage"] is explicitly set to null.
        /// </summary>
        [TestMethod]
        public void IndexNavClass_ActivePageIsNull_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = null;

            // Act
            var result = ManageNavPages.IndexNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that ChangePasswordNavClass returns "active" when the active page matches "ChangePassword" exactly.
        /// </summary>
        [TestMethod]
        public void ChangePasswordNavClass_ActivePageMatchesExactly_ReturnsActive()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "ChangePassword";

            // Act
            var result = ManageNavPages.ChangePasswordNavClass(viewContext);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that ChangePasswordNavClass returns "active" when the active page matches "ChangePassword" with different casing.
        /// </summary>
        [TestMethod]
        [DataRow("changepassword")]
        [DataRow("CHANGEPASSWORD")]
        [DataRow("ChangePassword")]
        [DataRow("ChAnGePaSsWoRd")]
        public void ChangePasswordNavClass_ActivePageMatchesCaseInsensitive_ReturnsActive(string activePage)
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = activePage;

            // Act
            var result = ManageNavPages.ChangePasswordNavClass(viewContext);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that ChangePasswordNavClass returns null when the active page does not match "ChangePassword".
        /// </summary>
        [TestMethod]
        [DataRow("Index")]
        [DataRow("ExternalLogins")]
        [DataRow("TwoFactorAuthentication")]
        [DataRow("SomeOtherPage")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("ChangePasswordExtra")]
        [DataRow("Change Password")]
        public void ChangePasswordNavClass_ActivePageDoesNotMatch_ReturnsNull(string activePage)
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = activePage;

            // Act
            var result = ManageNavPages.ChangePasswordNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that ChangePasswordNavClass returns null when ActivePage is not set in ViewData.
        /// </summary>
        [TestMethod]
        public void ChangePasswordNavClass_NoActivePageSet_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();

            // Act
            var result = ManageNavPages.ChangePasswordNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that ChangePasswordNavClass returns null when ActivePage is null in ViewData.
        /// </summary>
        [TestMethod]
        public void ChangePasswordNavClass_ActivePageIsNull_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = null;

            // Act
            var result = ManageNavPages.ChangePasswordNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that ChangePasswordNavClass returns null when ActivePage is set to a non-string value in ViewData.
        /// </summary>
        [TestMethod]
        public void ChangePasswordNavClass_ActivePageIsNonString_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = 123;

            // Act
            var result = ManageNavPages.ChangePasswordNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Minimal IView implementation for creating ViewContext in tests.
        /// </summary>
        private class FakeView : IView
        {
            public string Path => string.Empty;

            public System.Threading.Tasks.Task RenderAsync(ViewContext context)
            {
                return System.Threading.Tasks.Task.CompletedTask;
            }
        }

        /// <summary>
        /// Minimal ITempDataProvider implementation for creating ViewContext in tests.
        /// </summary>
        private class SessionStateTempDataProvider : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context)
            {
                return new Dictionary<string, object>();
            }

            public void SaveTempData(HttpContext context, IDictionary<string, object> values)
            {
            }
        }

        /// <summary>
        /// Tests that ExternalLoginsNavClass returns "active" when the ActivePage in ViewData matches "ExternalLogins".
        /// Input: ViewContext with ActivePage = "ExternalLogins"
        /// Expected: Returns "active"
        /// </summary>
        [TestMethod]
        public void ExternalLoginsNavClass_ActivePageMatchesExternalLogins_ReturnsActive()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = "ExternalLogins";

            // Act
            var result = ManageNavPages.ExternalLoginsNavClass(viewContext);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that ExternalLoginsNavClass returns "active" when the ActivePage in ViewData matches "ExternalLogins" with different casing.
        /// Input: ViewContext with ActivePage in various casings
        /// Expected: Returns "active" (case-insensitive comparison)
        /// </summary>
        [TestMethod]
        [DataRow("externallogins")]
        [DataRow("EXTERNALLOGINS")]
        [DataRow("ExTeRnAlLoGiNs")]
        [DataRow("externalLogins")]
        public void ExternalLoginsNavClass_ActivePageMatchesDifferentCasing_ReturnsActive(string activePage)
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = activePage;

            // Act
            var result = ManageNavPages.ExternalLoginsNavClass(viewContext);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that ExternalLoginsNavClass returns null when the ActivePage in ViewData does not match "ExternalLogins".
        /// Input: ViewContext with ActivePage set to different page names
        /// Expected: Returns null
        /// </summary>
        [TestMethod]
        [DataRow("Index")]
        [DataRow("ChangePassword")]
        [DataRow("TwoFactorAuthentication")]
        [DataRow("SomeOtherPage")]
        [DataRow("")]
        public void ExternalLoginsNavClass_ActivePageDoesNotMatch_ReturnsNull(string activePage)
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = activePage;

            // Act
            var result = ManageNavPages.ExternalLoginsNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that ExternalLoginsNavClass returns null when ActivePage is not set in ViewData.
        /// Input: ViewContext with no ActivePage key in ViewData
        /// Expected: Returns null
        /// </summary>
        [TestMethod]
        public void ExternalLoginsNavClass_ActivePageNotSet_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();

            // Act
            var result = ManageNavPages.ExternalLoginsNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that ExternalLoginsNavClass returns null when ActivePage in ViewData is null.
        /// Input: ViewContext with ActivePage = null
        /// Expected: Returns null
        /// </summary>
        [TestMethod]
        public void ExternalLoginsNavClass_ActivePageIsNull_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = null;

            // Act
            var result = ManageNavPages.ExternalLoginsNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that ExternalLoginsNavClass returns null when ActivePage in ViewData is a non-string value.
        /// Input: ViewContext with ActivePage set to non-string values (integer, object, etc.)
        /// Expected: Returns null (cast to string fails)
        /// </summary>
        [TestMethod]
        public void ExternalLoginsNavClass_ActivePageIsNonString_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext();
            viewContext.ViewData["ActivePage"] = 12345;

            // Act
            var result = ManageNavPages.ExternalLoginsNavClass(viewContext);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that the ExternalLogins property returns the expected constant value "ExternalLogins".
        /// </summary>
        [TestMethod]
        public void ExternalLogins_WhenAccessed_ReturnsExternalLoginsString()
        {
            // Arrange & Act
            var result = ManageNavPages.ExternalLogins;

            // Assert
            Assert.AreEqual("ExternalLogins", result);
        }
    }
}