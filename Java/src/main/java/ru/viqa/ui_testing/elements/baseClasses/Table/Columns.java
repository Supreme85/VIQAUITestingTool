package ru.viqa.ui_testing.elements.baseClasses.Table;

import ru.viqa.ui_testing.page_objects.VISite;
import ru.viqa.ui_testing.elements.interfaces.IHaveValue;
import org.openqa.selenium.By;
import org.openqa.selenium.WebElement;

import java.util.ArrayList;
import java.util.List;

import static ru.viqa.ui_testing.common.utils.LinqUtils.select;
import static java.lang.String.format;

/**
 * Created by 12345 on 26.10.2014.
 */
public class Columns<T extends IHaveValue> extends TableLine<T> {
    public Columns() {
        haveHeader = true;
        elementIndex = ElementIndexType.Nums;
    }

    protected String[] getHeadersAction() throws Exception {
        return select(table.getWebElement().findElements(By.xpath("//th")), WebElement::getText)
                .toArray(new String[1]);
    }

    private Exception getRowsException(String colName, Exception ex) throws Exception {
        return VISite.Alerting.throwError(format("Can't Get Column '%s'. Exception: %s", colName, ex));
    }

    public final List<Cell<T>> getRow(String name) throws Exception {
        try { return new ArrayList<>(select(table.getRows().headers(), rowName -> table.cell(name, rowName))); }
        catch (Exception ex) { throw getRowsException(name, ex); }
    }

    public List<Cell<T>> getRow(int num) throws Exception {
        int colsCount = -1;
        if (count > 0)
        colsCount = count;
        else if (headers != null && (headers.length > 0))
        colsCount = headers.length;
        if (colsCount > 0 && colsCount < num)
        throw VISite.Alerting.throwError(format("Can't Get Column '%s'. [num] > ColumnsCount(%s).", num, colsCount));
        try {
            List<Cell<T>> result = new ArrayList<>();
            for (int rowNum = 1; rowNum <= table.getRows().count(); rowNum++)
                result.add(table.cell(num, rowNum));
            return result;
        }
        catch (Exception ex) { throw getRowsException(num + "", ex); }
    }
}
