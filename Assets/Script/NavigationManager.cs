using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NavigationManager : MonoBehaviour
{
    static private List<T> CloneList<T>(List<T> old)
    {
        List<T> newList = new List<T>();
        for (int i = 0; i < old.Count; i++)
            newList.Add(old[i]);
        return newList;
    }

    public static NavigationManager ins;

    private static Vector3Int[] Around = new Vector3Int[] {
        new Vector3Int(-1, -1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(1, -1, 0),

        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),

        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
    };

    public Tilemap EnvMap;
    public Tilemap LadderMap;

    private Dictionary<Vector3Int, Ground> grounds;

    private void Awake() {
        ins = this;

        grounds = new Dictionary<Vector3Int, Ground>();
        ScanGroundConnectivity();
    }

    private void ScanGroundConnectivity() {
        BoundsInt bounds = EnvMap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            bool topWasAir = true;
            int height = 0;
            for (int y = bounds.yMax; y > bounds.yMin; y--)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (topWasAir)
                {
                    if (EnvMap.HasTile(pos))
                    {
                        grounds.Add(pos, new Ground(pos, height, EnvMap.CellToWorld(pos)));
                        height = 0;
                        topWasAir = false;
                    }
                    else
                        height++;
                }
                else
                {
                    if (!EnvMap.HasTile(pos))
                    {
                        topWasAir = true;
                        height = 1;
                    }
                }
            }
        }

        foreach (KeyValuePair<Vector3Int, Ground> pair in grounds)
        {
            List<Ground> connections = new List<Ground>();
            for (int i = 0; i < Around.Length; i++)
            {
                if (grounds.ContainsKey(pair.Key + Around[i]))
                {
                    connections.Add(grounds[pair.Key + Around[i]]);
                }
            }

            pair.Value.connections = connections.ToArray();
        }
    }

    public bool FindBlockUnderPosition(Vector3 pos, out Vector3Int position) {
        position = EnvMap.WorldToCell(pos);

        if (LadderMap.HasTile(position))
            return true;

        BoundsInt bounds = EnvMap.cellBounds;

        for (int y = position.y; y > bounds.yMin; y--)
        {
            position.y = y;
            if (grounds.ContainsKey(position))
                return true;
        }

        return false;
    }

    public Route[] FindNavigateRoute(Vector3Int start, Vector3Int end) {
        List<Route> routes;
        if (!grounds.ContainsKey(start)) {
            if (LadderMap.HasTile(start))
                routes = new List<Route>() { new Route(start, RouteType.Ladder) };
            else
                return new Route[0];
        }
        else
            routes = new List<Route>() { new Route(start, RouteType.Ground) };

        if (grounds.ContainsKey(end) || LadderMap.HasTile(end)) {
            List<Vector3Int> routeRepeatChecking = new List<Vector3Int>();
            routeRepeatChecking.Add(start);
            List<Route> connections = FindConnections(routeRepeatChecking, start, end);

            for (int i = 0; i < connections.Count; i++)
                routes.Add(connections[i]);
        }

        return routes.ToArray();
    }

    public List<Route> FindConnections(List<Vector3Int> routes, Vector3Int startPos, Vector3Int endPos, int count=0) {
        if (count > 300) {
            Debug.Log("REcursive overflow");
            return new List<Route>();
        }

        if (grounds.ContainsKey(startPos)) {
            Ground start = grounds[startPos];
            for (int i = 0; i < start.connections.Length; i++)
            {
                if (routes.Contains(start.connections[i].Position))
                    continue;

                if (start.connections[i].Position != endPos)
                {
                    List<Vector3Int> routesCopy = CloneList(routes);
                    routesCopy.Add(start.connections[i].Position);

                    List<Route> _newRoute = FindConnections(routesCopy, start.connections[i].Position, endPos, count + 1);

                    if (_newRoute.Count > 0) {
                        List<Route> newRoutes = new List<Route>();
                        newRoutes.Add(new Route(start.connections[i].Position, RouteType.Ground));
                        newRoutes.AddRange(_newRoute);
                        return newRoutes;
                    }
                }
                else {
                    return new List<Route>() { new Route(start.connections[i].Position, RouteType.Ground) };
                }
            }
        }

        // Find ladder go down
        BoundsInt ladderBound = LadderMap.cellBounds;
        if (LadderMap.HasTile(startPos))
        {
            List<Vector3Int> routesCopy = CloneList(routes);
            List<Route> ladders = new List<Route>();

            for (int y = startPos.y; y >= ladderBound.min.y - 1; y--) {
                Vector3Int pos = new Vector3Int(startPos.x, y, 0);
                routesCopy.Add(pos);
                ladders.Add(new Route(pos, RouteType.Ladder));

                if (endPos == pos)
                {
                    return ladders;
                }

                if (grounds.ContainsKey(pos))
                {
                    // if ground just hit
                    routesCopy.Add(pos);
                    List<Route> _newRoute = FindConnections(routesCopy, pos, endPos, count + 1);

                    if (_newRoute.Count > 0)
                    {
                        List<Route> newRoute = new List<Route>();
                        newRoute.AddRange(ladders);
                        newRoute.Add(new Route(pos, RouteType.Ground));
                        newRoute.AddRange(_newRoute);
                        return newRoute;
                    }
                }
                else if (!LadderMap.HasTile(pos))
                    break;
            }
        }

        // Find ladder go up
        startPos.y += 1;
        if (!routes.Contains(startPos) && LadderMap.HasTile(startPos))
        {
            List<Vector3Int> routesCopy = CloneList(routes);
            List<Route> ladders = new List<Route>();

            for (int y = startPos.y; y <= ladderBound.max.y; y++)
            {
                Vector3Int pos = new Vector3Int(startPos.x, y, 0);

                if (LadderMap.HasTile(pos))
                {
                    routesCopy.Add(pos);
                    ladders.Add(new Route(pos, RouteType.Ladder));

                    if (endPos == pos) {
                        if (grounds.ContainsKey(pos)) {
                            ladders.Add(new Route(pos + new Vector3Int(0, 1, 0), RouteType.Ladder));
                            ladders.Add(new Route(pos, RouteType.Ground));
                        }
                        return ladders;
                    }

                    if (grounds.ContainsKey(pos))
                    {
                        List<Route> _newRoutes = FindConnections(routesCopy, pos, endPos, count + 1);

                        if (_newRoutes.Count > 0)
                        {
                            List<Route> newRoute = new List<Route>();
                            newRoute.AddRange(ladders);
                            newRoute.Add(new Route(pos + new Vector3Int(0, 1, 0), RouteType.Ladder));
                            newRoute.Add(new Route(pos, RouteType.Ground));
                            newRoute.AddRange(_newRoutes);
                            return newRoute;
                        }
                    }
                }
                else
                    break;
            }
        }

        return new List<Route>();
    }

    public class Ground
    {
        public Vector3Int Position;
        public int Height;

        public Ground[] connections;

        public Vector3 actualPosition;

        public Ground(Vector3Int pos, int height, Vector3 _pos)
        {
            Position = pos;
            Height = height;

            actualPosition = _pos;
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (grounds != null)
        {
            Vector3 offset = new Vector3(0.5f, 0.95f);

            foreach (KeyValuePair<Vector3Int, Ground> pair in grounds)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(pair.Value.actualPosition + offset, new Vector3(1, 0.1f));

                Gizmos.color = Color.green;
                for (int i = 0; i < pair.Value.connections.Length; i++)
                {
                    Gizmos.DrawLine(pair.Value.actualPosition + offset,
                                    pair.Value.connections[i].actualPosition + offset);
                }
            }
        }
    }
    #endif

}

public enum RouteType
{
    Ground,
    Ladder,
}
public struct Route : System.IEquatable<Route>, System.IEquatable<Vector3Int>
{
    public Vector3Int Position;
    public RouteType Type;

    public Route(Vector3Int position, RouteType type)
    {
        Position = position;
        Type = type;
    }

    public bool Equals(Route other)
    {
        return Position == other.Position && Type == other.Type;
    }
    public bool Equals(Vector3Int other)
    {
        return Position == other;
    }
}