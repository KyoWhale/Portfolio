using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogPort : Port
{
    public DialogPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
    {
        m_EdgeConnector = new EdgeConnector<Edge>(new DefaultEdgeConnectorListener());
        this.AddManipulator(m_EdgeConnector);
    }

    /// <summary>
    /// Port 클래스에 구현되어 있는 DefaultEdgeConnectorListener와 동일함
    /// </summary>
    private class DefaultEdgeConnectorListener : IEdgeConnectorListener
    {
        private GraphViewChange m_GraphViewChange;
        private List<Edge> m_EdgesToCreate;
        private List<GraphElement> m_EdgesToDelete;

        public DefaultEdgeConnectorListener()
        {
            this.m_EdgesToCreate = new List<Edge>();
            this.m_EdgesToDelete = new List<GraphElement>();
            this.m_GraphViewChange.edgesToCreate = this.m_EdgesToCreate;
        }
    
        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }
    
        public void OnDrop(UnityEditor.Experimental.GraphView.GraphView graphView, Edge edge)
        {
            this.m_EdgesToCreate.Clear();
            this.m_EdgesToCreate.Add(edge);
            this.m_EdgesToDelete.Clear();
            if (edge.input.capacity == Port.Capacity.Single)
            {
                foreach (Edge connection in edge.input.connections)
                {
                if (connection != edge)
                    this.m_EdgesToDelete.Add((GraphElement) connection);
                }
            }
            if (edge.output.capacity == Port.Capacity.Single)
            {
                foreach (Edge connection in edge.output.connections)
                {
                if (connection != edge)
                    this.m_EdgesToDelete.Add((GraphElement) connection);
                }
            }
            if (this.m_EdgesToDelete.Count > 0)
                graphView.DeleteElements((IEnumerable<GraphElement>) this.m_EdgesToDelete);
            List<Edge> edgesToCreate = this.m_EdgesToCreate;
            if (graphView.graphViewChanged != null)
                edgesToCreate = graphView.graphViewChanged(this.m_GraphViewChange).edgesToCreate;
            foreach (Edge edge1 in edgesToCreate)
            {
                graphView.AddElement((GraphElement) edge1);
                edge.input.Connect(edge1);
                edge.output.Connect(edge1);
            }
        }
    }

    public override void Connect(Edge edge)
    {
        base.Connect(edge);
        
        (edge.output.node as DialogNode).SavePortConnections();
    }

    public override void Disconnect(Edge edge)
    {
        var outputNode = edge.output.node as DialogNode;

        base.Disconnect(edge);

        outputNode.SavePortConnections();
    }
}
