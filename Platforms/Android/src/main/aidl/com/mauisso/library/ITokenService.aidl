package com.mauisso.library;

interface ITokenService {
    String getAccessToken();
    String getRefreshToken();
    String getIdToken();
    boolean isAuthenticated();
    void logout();
}