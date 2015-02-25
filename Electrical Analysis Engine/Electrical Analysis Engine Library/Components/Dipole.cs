﻿using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    public abstract class Dipole: Item
    {

        protected Complex32 _current;//, voltage;
        public ComponentContainer Owner { get; set; }

        public bool IsConnectedToEarth
        {
            get
            {
                foreach (var item in Nodes)
                    if (item.IsReference)
                        return true;
                return false;
            }
        }

        public virtual Complex32 Current(Node referenceNode, Complex32? W = null)
        {
            if (referenceNode == Nodes[0])
                return _current;
            else
                return -_current;
        }

        /// <summary>
        /// Valor de la corriente de continua
        /// </summary>
        public virtual Complex32 current
        {
            get
            {
                return _current;
            }
            internal set
            {
                _current = value;
            }
        }

        public virtual Complex32 NortonCurrent(Node referenceNode, Complex32 ?W = null) {
            return 0;
        }
        public virtual Complex32 TheveninVoltage(Node referenceNode, Complex32? W = null)
        {
            return 0;
        }


        public virtual Complex32 voltage(Node ReferenceNode)
        {
            if (ReferenceNode == Nodes[0])
                return Voltage;
            if (ReferenceNode == Nodes[-1])
                return Voltage;
            return Complex32.NaN;
        }

        /// <summary>
        /// DC operating voltage
        /// </summary>
        public virtual Complex32 Voltage
        {
            get { return Nodes[0].Voltage - Nodes[1].Voltage; }
            //set { voltage = value; }
        }

        /// <summary>
        /// Listado de nodos de un componente. son 2 nada mas
        /// </summary>
        public List<Node> Nodes { get; protected set; }
        public int ReferenceNode { get; set; }

        public Dipole(ComponentContainer owner)
            : base()
        {
            Nodes = new List<Node>();
            Nodes.Add(new Node());
            Nodes.Add(new Node());
            Owner = owner;
        }
        
        public Node OtherNode(Node thisnode)
        {
            if (thisnode == Nodes[0])
                return Nodes[1];
            return Nodes[0];
        }

        public virtual Complex32 Impedance(Complex32? W = null)
        {
            return Complex32.Zero;
        }

        public virtual void Reset()
        {
            //Voltage = Complex32.Zero;
            current = Complex32.Zero;
        }

    }
}