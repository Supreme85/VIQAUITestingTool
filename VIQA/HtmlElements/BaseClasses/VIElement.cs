﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using VIQA.Common;
using VIQA.HtmlElements.Interfaces;
using VIQA.SiteClasses;
using Timer = VIQA.Common.Timer;

namespace VIQA.HtmlElements
{
    public class VIElement : VIElementsSet, IVIElement
    {
        public ISearchContext SearchContext
        {
            get
            {
                if (Context == null)
                    return WebDriver;
                var elements = WebDriver.FindElements(Context);
                return (elements.Count == 1) ? elements.First() : null;
            }
        }
        
        private string PrintLocator()
        {
            return string.Format("Context: '{0}'. Locator: '{1}'.", 
                Context != null ? Context.ToString() : "null", 
                _locator != null ? _locator.ToString() : "null");
        }

        public By Locator { 
            set { _locator = value; }
            get
            {
                var locator = string.IsNullOrEmpty(TemplateId)
                    ? _locator
                    : _locator.FillByTemplate(TemplateId);
                if (locator != null)
                    return locator;
                throw VISite.Alerting.ThrowError(DefaultLogMessage("Locator cannot be null"));
            }
        }
        public bool HaveLocator() { return _locator != null; }

        public IWebDriver WebDriver { get { return Site.WebDriver; } }
        public IWebDriverTimeouts Timeouts { get { return Site.WebDriverTimeouts; }}
        private IWebElement _webElement;
        private IWebElement WebElement { set
        {
            DropCache(); _webElement = value; }
            get { return _webElement; }
        }

        private int _defaultWaitTimeoutInSec { get { return Timeouts.WaitWebElementInSec; } }
        
        #region Temp Settings
        public void SetWaitTimeout(int waitTimeoutInSec) { _waitTimeoutInSec = waitTimeoutInSec; }
        private int? _waitTimeoutInSec = null;
        private string _templateId;
        public string TemplateId
        {
            set { DropCache(); _templateId = value; }
            private get { return _templateId;  }}


        #endregion

        protected int WaitTimeoutInSec
        {
            get
            {
                if (OpenPageName != null)
                {
                    if (OpenPageName != "")
                        Site.CheckPage(OpenPageName);
                    OpenPageName = null;
                    return Timeouts.WaitPageToLoadInSec;
                }
                return _waitTimeoutInSec ?? _defaultWaitTimeoutInSec;
            }
        }
        
        protected virtual string _typeName { get { return "Element type undefined"; } }
        public string FullName { get { return Name ?? _typeName + " with undefiened Name";} }

        private IWebElement CheckWebElementIsUnique(ReadOnlyCollection<IWebElement> webElements)
        {
            if (webElements.Count == 1)
                return webElements.First();
            if (webElements.Any())
                throw VISite.Alerting.ThrowError(LotOfFindElementsMessage(webElements));
            throw VISite.Alerting.ThrowError(CantFindElementMessage);
        }

        private IWebElement CheckWebElementIsDisplayed(IWebElement webElement)
        {
            var timer = new Timer();
            var timeoutInSec = Site.WebDriverTimeouts.WaitWebElementInSec;
            while (!timer.TimeoutPassed(timeoutInSec))
            {
                try { if (webElement.Displayed) break; }
                catch { }
            }
            if (webElement.Displayed)
                return webElement;
            throw VISite.Alerting.ThrowError(string.Format("Found element '{0}' stay invisible '{1}'. Please correct locator", PrintLocator(), timer.TimePassed()));
        }

        
        private string LotOfFindElementsMessage(ReadOnlyCollection<IWebElement> webElements) {
            return string.Format("Find {0} elements '{1}' but expected. Please correct locator '{2}'", webElements.Count, FullName, PrintLocator()); } 
        private string CantFindElementMessage { get {
            return string.Format("Can't find element '{0}' by selector '{1}'. Please correct locator", FullName, PrintLocator()); } }
        
        public ReadOnlyCollection<IWebElement> SearchElements(By locator = null)
        {
            try { return SearchContext.FindElements(locator ?? Locator); }
            catch { throw VISite.Alerting.ThrowError(CantFindElementMessage);}
        }
        private bool Wait(Func<bool> waitFunc, int timeoutInSec = -1)
        {
            var firstTime = true;
            if (timeoutInSec < 0)
                timeoutInSec = Site.WebDriverTimeouts.WaitWebElementInSec;
            var timer = new Timer();
            var result = false;
            do
            {
                if (firstTime)
                    firstTime = false;
                else
                    Thread.Sleep(Timeouts.RetryActionInMsec);
                try { result = waitFunc(); }
                catch { result = false; }
            }
            while (!result && !timer.TimeoutPassed(timeoutInSec));
            return result;
        }

        public bool WaitElementState(Func<IWebElement, bool> waitFunc, IWebElement webElement = null, int timeoutInSec = -1)
        {
            return Wait(() => waitFunc(webElement ?? CheckWebElementIsUnique(SearchElements())), timeoutInSec);
        }

        private ReadOnlyCollection<IWebElement> WaitWebElements(int timeout)
        {
            ReadOnlyCollection<IWebElement> foundElements = null;
            return (Wait(() => {
                try { foundElements = (SearchContext != null)
                        ? SearchElements()
                        : null; 
                } catch { foundElements = null; }
                return foundElements != null && foundElements.Any();
            }, timeout))
            ? foundElements
            : null;
        }

        public virtual bool IsPresent
        {
            get
            {
                WebDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
                var elements = SearchElements();
                WebDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(Site.WebDriverTimeouts.WaitWebElementInSec));
                return elements.Count > 0; 
            }
        }

        public virtual bool IsDisplayed
        {
            get
            {
                WebDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
                var elements = SearchElements();
                WebDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(Site.WebDriverTimeouts.WaitWebElementInSec));
                try { return CheckWebElementIsUnique(elements).Displayed; }
                catch (Exception ex)
                {
                    VISite.Logger.Event("IsDisplayed Failed with exception: " + ex);
                    return false;
                }
            }
        }
        
        public int CashDropTimes { get; set; }

        private void IsClearCashNeeded()
        {
            if (Site.SiteSettings.UseCache)
            {
                if (CashDropTimes != Site.SiteSettings.CashDropTimes) 
                    DropCache();
                return;
            }
            _webElement = null;
        }

        private void DropCache()
        {
            CashDropTimes = Site.SiteSettings.CashDropTimes;
            _webElement = null;
        }

        public IWebElement GetWebElement()
        {
            IsClearCashNeeded();
            if (WebElement != null)
                return WebElement;
            var timeoutInSec = WaitTimeoutInSec;
            var foundElements = WaitWebElements(timeoutInSec * 1000);
            if (foundElements == null)
                VISite.Alerting.ThrowError(CantFindElementMessage);
            WebElement = CheckWebElementIsUnique(foundElements);
            return CheckWebElementIsDisplayed(WebElement);
        }

        public IVIElement GetVIElement()
        {
            WebElement = GetWebElement();
            return this;
        }

        #region Constructors

        public VIElement()
        {
            DefaultNameFunc = () => _typeName + " with by selector " + PrintLocator();
        }

        public VIElement(string name)
        {
            Name = name;
        }

        public VIElement(string name, string cssSelector) : this(name) { Locator = By.CssSelector(cssSelector); }
        public VIElement(string name, By byLocator) : this(name) { Locator = byLocator; }
        public VIElement(By byLocator) : this() { Locator = byLocator; }
        public VIElement(string name, IWebElement webElement) : this(name) { WebElement = webElement; }
        public VIElement(IWebElement webElement) : this("", webElement) { }
        #endregion
        
        public string DefaultLogMessage(string text)
        {
            return text + string.Format(" (Name: '{0}', Type: '{1}', Locator: '{2}')", FullName, _typeName, PrintLocator());
        }

        private Func<string, Func<Object>, Func<Object, string>, Object> _defaultViActionR
        {
            get
            {
                return (text, viAction, logResult) =>
                {
                    VISite.Logger.Event(DefaultLogMessage(text));
                    var result = viAction();
                    if (logResult != null)
                        VISite.Logger.Event(logResult(result));
                    return result;
                }; } }

        private Func<string, Func<Object>, Func<Object, string>, Object> _viActionR;
        public Func<string, Func<Object>, Func<Object, string>, Object> VIActionR
        {
            set { _viActionR = value; }
            get { return _viActionR ?? _defaultViActionR; }
        }
        
        protected T DoVIAction<T>(string logActionName, Func<T> viAction, Func<T, string> logResult = null)
        {
            try { return (T)VIActionR(logActionName, () => viAction(), res => logResult != null ? logResult((T)res) : null); }
            catch (Exception ex)
            {
                throw VISite.Alerting.ThrowError(string.Format("Failed to do '{0}' action. Exception: {1}", logActionName, ex));
            }
        }

        //----
        public VIAction<Action<VIElement, string, Action>> DoViAction = new VIAction<Action<VIElement, string, Action>>(
            (viElement, text, viAction) =>
            {
                VISite.Logger.Event(viElement.DefaultLogMessage(text));
                viAction();
            });
        
        protected void DoVIAction(string logActionName, Action viAction)
        {
            try { DoViAction.Action(this, logActionName, viAction); }
            catch (Exception ex)
            {
                throw VISite.Alerting.ThrowError(string.Format("Failed to do '{0}' action. Exception: {1}", logActionName, ex));
            }
        }

        public static readonly Dictionary<Type, Type> InterfaceTypeMap = new Dictionary<Type, Type>
        {
            { typeof(IButton), typeof(Button) },
            { typeof(ICheckbox), typeof(Checkbox) },
            { typeof(ICheckList), typeof(CheckList) },
            { typeof(ILink), typeof(Link) },
            { typeof(ITextField), typeof(TextField) },
            { typeof(ITextArea), typeof(TextArea) },
            { typeof(IText), typeof(TextElement) },
            { typeof(IClickable), typeof(ClickableElement) },
            { typeof(IVIElement), typeof(VIElement) },
            { typeof(IRadioButtons), typeof(RadioButtons) },
            { typeof(IDropDown), typeof(DropDown) },
        };

        public static readonly Func<string, string, bool> DefaultCompareFunc = (a, e) => a == e;

        protected static string OpenPageName;

        public static void Init(VISite site)
        {
            OpenPageName = null;
            DefaultSite = site;
        }
    }
}
