using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVehicleRoutingProblem
{
    public class DVRPPathFinder
    {
        public List<Location> best_cycle;
        public List<double> bestArrivalsTimes;
        public double bestPathLen;
        public double cutOff;

        private int[] clientsId; // INDEKSY TABLICOWE
        private List<Location> act_cycle;
        private List<double> arrivalTimes;
        private bool[] used;
        private DVRP dvrp;
        private int depth; // głębokość rekurencji

        public DVRPPathFinder(int[] partialData, DVRP dvrp)
        {
            this.clientsId = partialData;
            this.act_cycle = new List<Location>();
            this.best_cycle = null;
            this.arrivalTimes = new List<double>();
            this.bestArrivalsTimes = null;
            this.used = new bool[partialData.Length];
            this.bestPathLen = Double.MaxValue;
            this.dvrp = dvrp;
            this.cutOff = 0.5;
            this.depth = partialData.Length;
        }

        public void FindCycle(int v, int k, double pathLen, double time, double capacity)
        {
            if (pathLen >= bestPathLen || time > dvrp.Depots[0].end) { return; }
            if (k == depth)
            {
                double distToDepot = DVRPHelper.Distance(act_cycle[act_cycle.Count - 1], act_cycle[0]);
                double timeToDepot = distToDepot / dvrp.Speed;
                
                if (pathLen + distToDepot < bestPathLen && time + timeToDepot < dvrp.Depots[0].end)
                {
                    bestPathLen = pathLen + distToDepot;
                    act_cycle.Add(dvrp.Locations[dvrp.Depots[0].locationID]);
                    best_cycle = new List<Location>(act_cycle);
                    arrivalTimes.Add(time + timeToDepot);
                    bestArrivalsTimes = new List<double>(arrivalTimes);

                    act_cycle.RemoveAt(act_cycle.Count - 1);
                    arrivalTimes.RemoveAt(arrivalTimes.Count - 1);
                }
                return;
            }


            for (int i = 0; i < clientsId.Length; i++)
            {
                if (!used[i])
                {
                    used[i] = true;

                    if (capacity + dvrp.Clients[clientsId[i]].size > 0)
                    {
                        double dist = dvrp.distances[v, dvrp.Clients[clientsId[i]].locationID];
                        double t = dist / dvrp.Speed;
                        double tWait = 0;//czas oczekiwania na przyjscie zgłoszenia
                        if (dvrp.Clients[clientsId[i]].time < cutOff * dvrp.Depots[0].end && time < dvrp.Clients[clientsId[i]].time)
                            tWait = dvrp.Clients[clientsId[i]].time - time;
                        arrivalTimes.Add(time + t + tWait);
                        act_cycle.Add(dvrp.Locations[dvrp.Clients[clientsId[i]].locationID]);
                        FindCycle(dvrp.Locations[dvrp.Clients[clientsId[i]].locationID].locationID, k + 1, pathLen + dist, time + t + tWait +dvrp.Clients[clientsId[i]].unld, capacity + dvrp.Clients[clientsId[i]].size);
                        act_cycle.RemoveAt(act_cycle.Count - 1);
                        arrivalTimes.RemoveAt(arrivalTimes.Count - 1);
                    }
                    else if (capacity + dvrp.Clients[clientsId[i]].size == 0)
                    {
                        double distToClient = dvrp.distances[v, dvrp.Clients[clientsId[i]].locationID];
                        double timeToClient = distToClient / dvrp.Speed;
                        double tWait = 0;//czas oczekiwania na przyjscie zgłoszenia
                        if (dvrp.Clients[clientsId[i]].time < cutOff * dvrp.Depots[0].end && time < dvrp.Clients[clientsId[i]].time)
                            tWait = dvrp.Clients[clientsId[i]].time - time;
                        arrivalTimes.Add(time + timeToClient + tWait);

                        double distToDepot = dvrp.distances[dvrp.Clients[clientsId[i]].locationID, dvrp.Depots[0].locationID];
                        double timeToDepot = distToDepot / dvrp.Speed;
                        arrivalTimes.Add(time + timeToClient + tWait + dvrp.Clients[clientsId[i]].unld + timeToDepot);

                        act_cycle.Add(dvrp.Locations[dvrp.Clients[clientsId[i]].locationID]);
                        act_cycle.Add(dvrp.Locations[dvrp.Depots[0].locationID]);
                        depth++;
                        FindCycle(0, k + 2, pathLen + distToClient + distToDepot, time + timeToClient + tWait + dvrp.Clients[clientsId[i]].unld + timeToDepot, dvrp.Capacities);
                        depth--;
                        act_cycle.RemoveAt(act_cycle.Count - 1);
                        act_cycle.RemoveAt(act_cycle.Count - 1);
                        arrivalTimes.RemoveAt(arrivalTimes.Count - 1);
                        arrivalTimes.RemoveAt(arrivalTimes.Count - 1);
                    }
                    else
                    {
                        double distCD = 0; // distance from prev client to depot
                        double distDC = 0; //disttance from depot to client
                        if (act_cycle.Count > 0)
                        {
                            distCD = dvrp.distances[act_cycle[act_cycle.Count - 1].locationID, dvrp.Depots[0].locationID];
                        }
                        distDC = dvrp.distances[dvrp.Depots[0].locationID, dvrp.Clients[clientsId[i]].locationID];

                        double tCD = distCD / dvrp.Speed;
                        arrivalTimes.Add(time + tCD);
                        double tDC = distDC / dvrp.Speed;
                        double tWait = 0;//czas oczekiwania na przyjscie zgłoszenia
                        if (dvrp.Clients[clientsId[i]].time < cutOff * dvrp.Depots[0].end && time < dvrp.Clients[clientsId[i]].time)
                            tWait = dvrp.Clients[clientsId[i]].time - time;
                        arrivalTimes.Add(time + tCD + tDC + tWait);

                        act_cycle.Add(dvrp.Locations[dvrp.Depots[0].locationID]);
                        depth++;
                        act_cycle.Add(dvrp.Locations[dvrp.Clients[clientsId[i]].locationID]);

                        FindCycle(dvrp.Clients[clientsId[i]].locationID, k + 2, pathLen + distCD + distDC, time + tCD + tDC + tWait, dvrp.Capacities + dvrp.Clients[clientsId[i]].size);
                        depth--;
                        act_cycle.RemoveAt(act_cycle.Count - 1);
                        act_cycle.RemoveAt(act_cycle.Count - 1);

                        arrivalTimes.RemoveAt(arrivalTimes.Count - 1);
                        arrivalTimes.RemoveAt(arrivalTimes.Count - 1);
                    }
                    used[i] = false;
                }
            }
        }
    }
}
