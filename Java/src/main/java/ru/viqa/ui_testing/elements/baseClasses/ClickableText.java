package ru.viqa.ui_testing.elements.baseClasses;

import ru.viqa.ui_testing.elements.interfaces.*;
import org.openqa.selenium.By;
import org.openqa.selenium.WebElement;
import ru.viqa.ui_testing.elements.simpleElements.Text;

/**
 * Created by 12345 on 02.10.2014.
 */
public class ClickableText extends Clickable implements IText{
    private Text TextElement;
    private Text getTexElement() throws Exception {
        if (TextElement == null) {
            TextElement = new Text(getName() + " text", getLocator());
            TextElement.setSite(getSite());
            TextElement.Context = Context;
        }
        return  TextElement;
    }

    public ClickableText() throws Exception{ super(); }
    public ClickableText(String name) throws Exception { super(name); }
    public ClickableText(String name, String cssSelector) throws Exception {
        super(name, cssSelector); Init(name, By.cssSelector(cssSelector)); }
    public ClickableText(String name, By byLocator) throws Exception {
        super(name, byLocator); Init(name, byLocator); }
    public ClickableText(By byLocator) throws Exception { super(byLocator); Init("", byLocator); }
    public ClickableText(By byClickLocator, By byTextLocator) throws Exception {
        super(byClickLocator); Init("", byTextLocator); }
    public ClickableText(String name, WebElement webElement) throws Exception {
        super(name, webElement); Init(name, webElement); }
    public ClickableText(WebElement webElement) throws Exception {
        super(webElement); Init("", webElement); }

    private void Init(String name, By byLocator) throws Exception {
        TextElement = new Text(name + " text", byLocator);
        TextElement.setSite(getSite());
        TextElement.Context = Context;
    }
    private void Init(String name, WebElement webElement) throws Exception {
        TextElement = new Text(name + " text", webElement);
        TextElement.setSite(getSite());
        TextElement.Context = Context;
    }

    protected String getTextAction() throws Exception {
        return getTexElement().getText(); }
    protected String getValueAction() throws Exception { return getTextAction(); }
    public final String getValue() throws Exception {
        return doVIActionResult("Get value", this::getValueAction, text -> text);
    }
    @Deprecated
    public final void setValue(String value) throws Exception {  }

    public final String getText() throws Exception{
        return doVIActionResult("Get text", this::getTextAction, text -> text);
    }
}
