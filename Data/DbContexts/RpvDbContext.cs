﻿using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace raw_ws.Data.DbContexts
{
    public class RpvDbContext
    {
        public string ConnRpvGestion { get; }
        public string ConnMsm { get; }
        public string ConnIntranet { get; }
        public string ConnPg { get; }
        public string ConnCCEP { get; }
        public string ConnCCEP_AC { get; }
        public string ConnRaw { get; }
        public string ConnKimberly { get; }


        public RpvDbContext(IConfiguration configuration)
        {
            ConnRpvGestion = configuration["ConnectionStrings:DatabaseConnection_RPVGESTION"];
            ConnMsm = configuration["ConnectionStrings:DatabaseConnection_MSM"];
            ConnIntranet = configuration["ConnectionStrings:DatabaseConnection_INTRANET"];
            ConnPg = configuration["ConnectionStrings:DatabaseConnection_PG"];
            ConnCCEP = configuration["ConnectionStrings:DatabaseConnection_CCEP"];
            ConnCCEP_AC = configuration["ConnectionStrings:DatabaseConnection_CCEP_AC"];
            ConnRaw = configuration["ConnectionStrings:DatabaseConnection_RAW"];
            ConnKimberly = configuration["ConnectionStrings:DatabaseConnection_KIMBERLY"];
            //Conn = new SqlConnection(configuration["ConnectionStrings:DatabaseConnection_LOCAL"]);
        }
    }
}