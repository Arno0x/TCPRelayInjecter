TCPRelayInjecter
============

**UPDATE** : You should check a more recent version of the TCPRelayInjecter that uses a different (simpler) technique of DLLinjection as well as supports 32 and 64 bits target processes [TCPRelayInjecter2](https://github.com/Arno0x/TCPRelayInjecter2)

Author: [Arno0x](https://twitter.com/Arno0x0x).

This project is heavily based on [SharpNeedle](https://github.com/ChadSki/SharpNeedle).


The tool is used to inject a "TCP Forwarder" managed assembly (*TCPRelay.dll*) into an unmanaged 32 bits process.

Note: TCPRelayInjecter only supports 32-bits target processes and only relays TCP connections.

Background and context
----------------

I created this tool in order to bypass Windows local firewall rules preventing some inbound connections I needed (*in order to perform some relay and/or get a MiTM position*). As a non-privileged user, firewall rules could not be modified or added.

The idea is to find a process running as the same standard (*non-privileged*) user **AND** allowed to receive any network connection, or at least the ones we need:
`netsh advfirewall firewall show rule name=all`

From there we just have to inject a TCP Forwarder assembly in it, passing it some arguments like a local port to listen to, a destination port and an optionnal destination IP to forward the traffic to.

Compile
----------------
Open the `TCPRelayInjecter.sln` file with Visual Studio, compile the solution. Tested and working with Visual Studio Community 2019.

Usage
----------------

Prior to running the tool, ensure the 3 binary files are in the same path:
  - TcpRelayInjecter.exe
  - Bootstrapper.dll
  - TCPRelay.dll

 Then use the following command line:

`TcpRelayInjecter.exe <target_process_name> <listening_port> <destination_port> [destination_IP]`

  - target_process_name: The name of the executable we want to inject the TCP Forwarder into
  - listening_port: the TCP port to use for listening for inbound connections
  - destination_port: the TCP port to which forward the traffic (typically another process would be listening on that port)
  - destination_IP: *Optionnal*,  the destination IP to which forward the traffic, if not specified, defaults to localhost

License
----------------

Just as requested by the SharpNeedle project, this project is released under the 2-clause BSD license.
