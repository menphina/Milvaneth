using System;
using System.Collections.Generic;
using System.Net;

namespace Thaliak.Network.Utilities
{
    public class Connection : IEquatable<Connection>
    {
        public IPEndPoint LocalEndPoint { get; }
        public IPEndPoint RemoteEndPoint { get; }
        public Connection Reverse => new Connection(RemoteEndPoint, LocalEndPoint);

        public Connection(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
        }

        public bool Equals(Connection connection)
        {
            return LocalEndPoint.Equals(connection.LocalEndPoint) && RemoteEndPoint.Equals(connection.RemoteEndPoint);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return obj is Connection connection && LocalEndPoint.Equals(connection.LocalEndPoint) && RemoteEndPoint.Equals(connection.RemoteEndPoint);
        }

        public override int GetHashCode()
        {
            return (LocalEndPoint.GetHashCode() + 0x0609) ^ RemoteEndPoint.GetHashCode();
        }

        public override string ToString()
        {
            return $"{LocalEndPoint} -> {RemoteEndPoint}";
        }
    }

    public class ConnectionEqualityComparer : IEqualityComparer<Connection>
    {
        public bool Equals(Connection b1, Connection b2)
        {
            if (object.ReferenceEquals(b1, b2))
                return true;

            if (b1 is null || b2 is null)
                return false;

            return b1.Equals(b2);
        }

        public int GetHashCode(Connection connection) => connection.GetHashCode();
    }
}
