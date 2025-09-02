

resource "aws_docdb_cluster_parameter_group" "mongo_params" {
  family = "docdb5.0"
  name   = "${var.environment}-mongo-param-val"

  parameter {
    name  = "tls"
    value = "tls1.2+"
  }
}


resource "aws_docdb_subnet_group" "mongo_grp" {
  name       = "${var.environment}-mongo-subnet-group"
  subnet_ids = var.data_subnets_ids
}


resource "aws_docdb_cluster" "mongo_cluster" {
  skip_final_snapshot             = true
  db_subnet_group_name            = aws_docdb_subnet_group.mongo_grp.name
  cluster_identifier              = "${var.environment}-mongo-app-cluster"
  engine                          = "docdb"
  master_username                 = "adminmongo"
  manage_master_user_password     = true
  db_cluster_parameter_group_name = aws_docdb_cluster_parameter_group.mongo_params.name
  storage_encrypted               = true
  kms_key_id                      = var.kms_key_id
  vpc_security_group_ids          = [var.app_sg_id]
  storage_type                    = "iopt1"
  deletion_protection             = var.delete_protection_enabled
  apply_immediately               = true
  backup_retention_period         = 1

}


resource "aws_docdb_cluster_instance" "mongo_instance" {
  count              = var.mongo_node_count
  identifier         = "${var.environment}-mongo-instance"
  cluster_identifier = aws_docdb_cluster.mongo_cluster.id
  instance_class     = var.mongo_instance_type
}
