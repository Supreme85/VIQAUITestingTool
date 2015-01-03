package ru.viqa.ui_testing.page_objects;

import org.openqa.selenium.WebDriver;

import java.util.*;

/**
 * Created by roman.i on 25.09.2014.
 */
public class Navigation {

    private VISite _site;
    private WebDriver getWebDriver() throws Exception { return _site.getWebDriver(); }

    public Navigation(VISite site)
    {
        pagesHistory = new ArrayList<>();
        _currentPageNum = -1;
        _site = site;
    }

    public ArrayList<VIPage> pagesHistory;
    private int _currentPageNum;
    public void incCurrentPageNum() { _currentPageNum++; }
    public void decCurrentPageNum() { _currentPageNum--; }
    public VIPage getCurrentPage() { return pagesHistory.toArray(new VIPage[pagesHistory.size()])[_currentPageNum]; }
    public String WindowHandle;

    public void openPage(String uri) throws Exception {
        new VIPage("Page with url " + VIPage.getUrlValue(uri, _site), uri, "", _site).open();
    }

    public void processNewPage(VIPage page) throws Exception {
        WindowHandle = getWebDriver().getWindowHandle();
        _site.SiteSettings.dropCash();
        pagesHistory.add(page);
        incCurrentPageNum();
    }

    public void processGoBack() throws Exception {
        _site.SiteSettings.dropCash();
        decCurrentPageNum();
    }
    public void processGoForward() throws Exception {
        _site.SiteSettings.dropCash();
        incCurrentPageNum();
    }

    public void processRefreshPage() throws Exception {
        _site.SiteSettings.dropCash();
    }
}
