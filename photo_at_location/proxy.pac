function FindProxyForURL(url,host)
{
    var resolved_ip = dnsResolve(host);
 //   var resolvable = isResolvable(host);
    if  (dnsDomainIs(host,"www.update.microsoft.com")||
	 dnsDomainIs(host,"www.windowsupdate.com")||
         dnsDomainIs(host,"windowsupdate.microsoft.com")||
         dnsDomainIs(host,"update.microsoft.com")||
         dnsDomainIs(host,"www.download.windowsupdate.com")||
         dnsDomainIs(host,"download.windowsupdate.com")||
         dnsDomainIs(host,"au.download.windowsupdate.com") ||
         dnsDomainIs(host,"www.download.windowsupdate.com") ||
         dnsDomainIs(host,"genuine.microsoft.com") ||
         dnsDomainIs(host,"v4.windowsupdate.microsoft.com") ||
         dnsDomainIs(host,"v5.windowsupdate.microsoft.com") ||
         dnsDomainIs(host,"crl.microsoft.com") ||
         dnsDomainIs(host,"c.microsoft.com") ||
         dnsDomainIs(host,"www.microsoft.com") ||
         dnsDomainIs(host,"download.microsoft.com") ||
         dnsDomainIs(host,"stats.update.microsoft.com"))    
                  return "PROXY 10.160.3.88:8080";

    else if (isResolvable(host) == 0 ||
	     isPlainHostName(host) ||
	     dnsDomainLevels(host) == 0 ||
             isInNet(host, "10.0.0.0", "255.0.0.0") ||
	     isInNet(host, "172.16.0.0", "255.240.0.0") || 
	     isInNet(host, "192.168.0.0", "255.255.0.0") || 
	     isInNet(host, "127.0.0.0", "255.255.255.0") ||
	     isInNet(resolved_ip, "10.0.0.0", "255.0.0.0") ||
	     isInNet(resolved_ip, "172.16.0.0", "255.240.0.0") || 
	     isInNet(resolved_ip, "192.168.0.0", "255.255.0.0") || 
	     isInNet(resolved_ip, "127.0.0.0", "255.255.255.0"))
                 return "DIRECT";

    else if (isInNet(myIpAddress(), "10.52.0.0", "255.255.0.0"))
	         return "PROXY proxy.cht.com.tw:8080";

    else if (isInNet(myIpAddress(), "10.48.0.0", "255.240.0.0")||
                            isInNet(myIpAddress(), "10.96.0.0", "255.240.0.0"))
                return "PROXY sproxy.cht.com.tw:8080";      
	
    else	 return "PROXY proxy.cht.com.tw:8080";
}

