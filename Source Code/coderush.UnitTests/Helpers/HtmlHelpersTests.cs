using System;
using coderush.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace coderush.Helpers.UnitTests
{
    /// <summary>
    /// Unit tests for the HtmlHelpers static class.
    /// </summary>
    [TestClass]
    public class HtmlHelpersTests
    {
        /// <summary>
        /// Tests that IsSelected returns the cssClass when both controller and action match the current route.
        /// </summary>
        [TestMethod]
        public void IsSelected_MatchingControllerAndAction_ReturnsCssClass()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "Index", "selected");

            // Assert
            Assert.AreEqual("selected", result);
        }

        /// <summary>
        /// Tests that IsSelected returns empty string when controller does not match.
        /// </summary>
        [TestMethod]
        public void IsSelected_NonMatchingController_ReturnsEmpty()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Account", "Index", "selected");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Tests that IsSelected returns empty string when action does not match.
        /// </summary>
        [TestMethod]
        public void IsSelected_NonMatchingAction_ReturnsEmpty()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "About", "selected");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Tests that IsSelected returns empty string when neither controller nor action match.
        /// </summary>
        [TestMethod]
        public void IsSelected_NonMatchingControllerAndAction_ReturnsEmpty()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Account", "Login", "selected");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Tests that IsSelected defaults cssClass to "active" when cssClass is null and route matches.
        /// </summary>
        [TestMethod]
        public void IsSelected_NullCssClassWithMatch_ReturnsActive()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "Index", null);

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected defaults cssClass to "active" when cssClass is empty and route matches.
        /// </summary>
        [TestMethod]
        public void IsSelected_EmptyCssClassWithMatch_ReturnsActive()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "Index", "");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected does not default cssClass to "active" when cssClass is whitespace and route matches.
        /// </summary>
        [TestMethod]
        public void IsSelected_WhitespaceCssClassWithMatch_ReturnsWhitespace()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "Index", "   ");

            // Assert
            Assert.AreEqual("   ", result);
        }

        /// <summary>
        /// Tests that IsSelected returns empty string when cssClass is null and route does not match.
        /// </summary>
        [TestMethod]
        public void IsSelected_NullCssClassWithoutMatch_ReturnsEmpty()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Account", "Login", null);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Tests that IsSelected uses current controller when controller parameter is null.
        /// </summary>
        [TestMethod]
        public void IsSelected_NullController_UsesCurrentController()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected(null, "Index", "active");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected uses current controller when controller parameter is empty.
        /// </summary>
        [TestMethod]
        public void IsSelected_EmptyController_UsesCurrentController()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("", "Index", "active");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected uses current action when action parameter is null.
        /// </summary>
        [TestMethod]
        public void IsSelected_NullAction_UsesCurrentAction()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", null, "active");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected uses current action when action parameter is empty.
        /// </summary>
        [TestMethod]
        public void IsSelected_EmptyAction_UsesCurrentAction()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "", "active");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected uses both current controller and action when both parameters are null.
        /// </summary>
        [TestMethod]
        public void IsSelected_NullControllerAndAction_UsesCurrentValues()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected(null, null, "active");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected handles case-sensitive comparison correctly when controller does not match by case.
        /// </summary>
        [TestMethod]
        public void IsSelected_DifferentCaseController_ReturnsEmpty()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("home", "Index", "active");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Tests that IsSelected handles case-sensitive comparison correctly when action does not match by case.
        /// </summary>
        [TestMethod]
        public void IsSelected_DifferentCaseAction_ReturnsEmpty()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "index", "active");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Tests that IsSelected handles special characters in controller name correctly.
        /// </summary>
        [TestMethod]
        public void IsSelected_SpecialCharactersInController_MatchesCorrectly()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home-Controller_123";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home-Controller_123", "Index", "active");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected handles special characters in action name correctly.
        /// </summary>
        [TestMethod]
        public void IsSelected_SpecialCharactersInAction_MatchesCorrectly()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index-Action_123";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "Index-Action_123", "active");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected handles special characters in cssClass correctly.
        /// </summary>
        [TestMethod]
        public void IsSelected_SpecialCharactersInCssClass_ReturnsCorrectly()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "Index", "active-class_123");

            // Assert
            Assert.AreEqual("active-class_123", result);
        }

        /// <summary>
        /// Tests that IsSelected handles very long controller name correctly.
        /// </summary>
        [TestMethod]
        public void IsSelected_VeryLongController_MatchesCorrectly()
        {
            // Arrange
            var longController = new string('A', 1000);
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = longController;
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected(longController, "Index", "active");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected handles very long action name correctly.
        /// </summary>
        [TestMethod]
        public void IsSelected_VeryLongAction_MatchesCorrectly()
        {
            // Arrange
            var longAction = new string('B', 1000);
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = longAction;
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", longAction, "active");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected handles very long cssClass correctly.
        /// </summary>
        [TestMethod]
        public void IsSelected_VeryLongCssClass_ReturnsCorrectly()
        {
            // Arrange
            var longCssClass = new string('C', 1000);
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "Index", longCssClass);

            // Assert
            Assert.AreEqual(longCssClass, result);
        }

        /// <summary>
        /// Tests that IsSelected handles null values in RouteData correctly when casting to string.
        /// </summary>
        [TestMethod]
        public void IsSelected_NullRouteDataValues_HandlesCorrectly()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = null;
            routeData.Values["action"] = null;
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected(null, null, "active");

            // Assert
            Assert.AreEqual("active", result);
        }

        /// <summary>
        /// Tests that IsSelected handles whitespace controller parameter correctly (not treated as empty).
        /// </summary>
        [TestMethod]
        public void IsSelected_WhitespaceController_DoesNotUseCurrentController()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("   ", "Index", "active");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Tests that IsSelected handles whitespace action parameter correctly (not treated as empty).
        /// </summary>
        [TestMethod]
        public void IsSelected_WhitespaceAction_DoesNotUseCurrentAction()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";
            var viewContext = new ViewContext { RouteData = routeData };
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = mockHtmlHelper.Object.IsSelected("Home", "   ", "active");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }
    }
}