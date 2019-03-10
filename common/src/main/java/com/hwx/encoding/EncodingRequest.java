//package com.hwx.encoding;
//
//import javax.servlet.http.HttpServletRequest;
//import javax.servlet.http.HttpServletRequestWrapper;
//
//public class EncodingRequest extends HttpServletRequestWrapper {
//
//    public EncodingRequest(HttpServletRequest request) {
//        super(request);
//    }
//
//
//    @Override
//    public String getParameter(String name) {
//        String value = super.getParameter(name);
//        try {
//            return new String(value.getBytes("iso-8859-1"), "utf-8");
//        }catch (Exception ex){
//            return value;
//        }
//    }
//}
