//package com.hwx.encoding;
//
//import javax.servlet.*;
//import javax.servlet.annotation.WebFilter;
//import javax.servlet.http.HttpServletRequest;
//import java.io.IOException;
//
//@WebFilter(filterName = "EncodingFilter",urlPatterns = "/*")
//public class EncodingFilter implements Filter {
//    public void destroy() {
//    }
//
//    public void doFilter(ServletRequest req, ServletResponse resp, FilterChain chain) throws ServletException, IOException {
//        HttpServletRequest hreq= ((HttpServletRequest) req);
//        if(hreq.getMethod().equals("POST")){
//            req.setCharacterEncoding("utf-8");
//        }else {
//            req = new EncodingRequest(hreq);
//        }
//        chain.doFilter(req, resp);
//    }
//
//    public void init(FilterConfig config) throws ServletException {
//
//    }
//
//}
