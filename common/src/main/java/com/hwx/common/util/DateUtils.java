package com.hwx.common.util;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;

public final class DateUtils {
    public static final String LONG_DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";
    public static final String SHORT_DATE_FOMAT = "yyyy-MM-dd";

    private static ThreadLocal<Calendar> calendarThreadLocal= ThreadLocal.withInitial(()->Calendar.getInstance());
    private static ThreadLocal<DateFormat> longThreadLocal= ThreadLocal.withInitial(()->new SimpleDateFormat(LONG_DATE_FORMAT));
    private static ThreadLocal<DateFormat> shortThreadLocal= ThreadLocal.withInitial(()->new SimpleDateFormat(SHORT_DATE_FOMAT));

    public static String toString(Date date){
       return toString(date,LONG_DATE_FORMAT);
    }

    private static DateFormat getDataFormat(String format){
        switch (format){
            case LONG_DATE_FORMAT:
                return longThreadLocal.get();
            case SHORT_DATE_FOMAT:
                return shortThreadLocal.get();
            default:
                return new SimpleDateFormat(format);
        }
    }

    public static String toString(Date date,String format){
        //return new SimpleDateFormat(format).format(date);
        return  getDataFormat(format).format(date);//优化：性能提高一倍以上
    }

    public static Date toDate(String date){
        /*try {
            return toDate(date,LONG_DATE_FORMAT);
        }catch (Exception ex){
            return toDate(date,SHORT_DATE_FOMAT);
        }*/
        if(date.length()>10){
            return toDate(date,LONG_DATE_FORMAT);
        }else {
            return toDate(date,SHORT_DATE_FOMAT);
        }
    }

    public static Date toDate(String date,String format){
        try {
            return getDataFormat(format).parse(date);
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }

    public static Calendar getCalendar(Date date){
        //Calendar calendar = Calendar.getInstance();
        Calendar calendar = calendarThreadLocal.get();//优化：性能提高一倍以上
        calendar.setTime(date);
        return calendar;
    }

    public static Date getDate(Date date){
        return toDate(toString(date,SHORT_DATE_FOMAT));
    }

    public static int get(Date date,int filed){
        return getCalendar(date).get(filed);
    }

    public static int getDay(Date date){
        return get(date,Calendar.DAY_OF_MONTH);
    }

    public static int getHour(Date date){
        return get(date,Calendar.HOUR_OF_DAY);
    }

    public static int getMinute(Date date){
        return get(date,Calendar.MINUTE);
    }

    public static int getSecond(Date date){
        return get(date,Calendar.SECOND);
    }

    public static int getMaxDayOfMonth(Date date){
       return getCalendar(date).getActualMaximum(Calendar.DAY_OF_MONTH);
    }

    public static Date add(Date date,int field,int value){
        Calendar calendar = getCalendar(date);
        calendar.add(field,value);
        return calendar.getTime();
    }

    public static Date addMonths(Date date,int value){
        return add(date,Calendar.MONTH,value);
    }

    public static Date addDays(Date date,int value){
        return add(date,Calendar.DAY_OF_MONTH,value);
    }

    public static Date addHours(Date date,int value){
        return add(date,Calendar.HOUR_OF_DAY,value);
    }

}

